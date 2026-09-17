import fs from 'node:fs';
import { fileURLToPath } from 'node:url';
import { spawnSync } from 'node:child_process';
import assert from 'node:assert/strict';

// Explicit target: the PostgreSQL container defined by backend/docker-compose.yml.
const input = JSON.parse(fs.readFileSync(new URL('./normalized/questions.json', import.meta.url), 'utf8'));
assert(input.length > 0);
assert.equal(new Set(input.map(q => q.id)).size, input.length);
assert.equal(new Set(input.map(q => q.slug)).size, input.length);
for (const q of input) {
  assert(/^[0-9a-f-]{36}$/.test(q.id));
  assert(q.question && q.slug && Array.isArray(q.departments) && q.departments.length);
  assert(Array.isArray(q.topics) && Array.isArray(q.sources) && Array.isArray(q.answerVariants));
}
const validation = spawnSync(process.execPath, [fileURLToPath(new URL('./normalize.mjs', import.meta.url)), '--check'], { encoding: 'utf8' });
if (validation.status !== 0) throw new Error(validation.stderr || validation.stdout);
const copyRows = input.map(q => '"' + JSON.stringify(q).replaceAll('"', '""') + '"').join('\n');
const sql = `
\\set ON_ERROR_STOP on
BEGIN;
SELECT pg_advisory_xact_lock(719430701);
DO $$ BEGIN
  IF current_database() <> 'gdsc_sharing_platform' THEN
    RAISE EXCEPTION 'Unexpected target database';
  END IF;
END $$;
CREATE TABLE IF NOT EXISTS gdsc."InterviewQuestions" (
  "Id" uuid PRIMARY KEY,
  "Data" jsonb NOT NULL CHECK (jsonb_typeof("Data") = 'object'),
  "Question" text GENERATED ALWAYS AS ("Data"->>'question') STORED NOT NULL,
  "Slug" text GENERATED ALWAYS AS ("Data"->>'slug') STORED NOT NULL UNIQUE,
  "Level" text GENERATED ALWAYS AS ("Data"->>'level') STORED NOT NULL,
  "Access" text GENERATED ALWAYS AS ("Data"->>'access') STORED NOT NULL,
  "NeedsReview" boolean GENERATED ALWAYS AS (("Data"->>'needsReview')::boolean) STORED NOT NULL,
  "Status" text NOT NULL CHECK ("Status" IN ('draft', 'published')),
  "CreatedAt" timestamptz NOT NULL DEFAULT now(),
  CHECK ("Data"->>'id' IS NOT NULL AND "Id" = ("Data"->>'id')::uuid)
);
CREATE TEMP TABLE incoming_questions (data jsonb NOT NULL) ON COMMIT DROP;
COPY incoming_questions(data) FROM STDIN WITH (FORMAT csv);
${copyRows}
\\.
INSERT INTO gdsc."InterviewQuestions" ("Id", "Data", "Status")
SELECT (data->>'id')::uuid, data,
  CASE WHEN (data->>'needsReview')::boolean
    OR data->'answer' = 'null'::jsonb
    OR NOT EXISTS (SELECT 1 FROM jsonb_each(data->'answer') e WHERE jsonb_array_length(e.value) > 0)
    THEN 'draft' ELSE 'published' END
FROM incoming_questions
ON CONFLICT ("Id") DO NOTHING;
DO $$ BEGIN
  IF EXISTS (
    SELECT 1 FROM incoming_questions i
    LEFT JOIN gdsc."InterviewQuestions" q ON q."Id" = (i.data->>'id')::uuid
    WHERE q."Data" IS DISTINCT FROM i.data
  ) THEN RAISE EXCEPTION 'Existing data differs from dataset; transaction rolled back without overwriting'; END IF;
END $$;
CREATE INDEX IF NOT EXISTS "IX_InterviewQuestions_Level" ON gdsc."InterviewQuestions" ("Level");
SELECT count(*) AS verified_dataset_rows FROM gdsc."InterviewQuestions" q
JOIN incoming_questions i ON q."Id" = (i.data->>'id')::uuid AND q."Data" = i.data;
SELECT "Status", count(*) FROM gdsc."InterviewQuestions" GROUP BY "Status" ORDER BY "Status";
COMMIT;
`;
const result = spawnSync('docker', ['exec', '-i', 'gdsc-sharing-platform-postgres', 'sh', '-c', 'exec psql -X -U "$POSTGRES_USER" -d "$POSTGRES_DB"'], { input: sql, encoding: 'utf8', maxBuffer: 8 * 1024 * 1024 });
process.stdout.write(result.stdout || '');
process.stderr.write(result.stderr || '');
if (result.error) throw result.error;
process.exitCode = result.status ?? 1;
