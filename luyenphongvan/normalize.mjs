import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import assert from 'node:assert/strict';
import { fileURLToPath } from 'node:url';

const root = path.dirname(fileURLToPath(import.meta.url));
const check = process.argv.includes('--check');
const files = fs.readdirSync(root).filter(f => f.endsWith('.json')).sort();
const missing = { 'ai.json': 'ai-ml', 'ba.json': 'business-analysis', 'backend.json': 'backend', 'database.json': 'data-and-messaging', 'devops-cloud.json': 'infrastructure', 'frontend.json': 'frontend' };
const aliases = { 'AI&ML': 'ai-ml', infracstruture: 'infrastructure', carreer: 'career', 'data and messaging': 'data-and-messaging', 'quality & security': 'quality-security' };
const allowed = new Set(['ai-ml', 'business-analysis', 'backend', 'frontend', 'mobile', 'base', 'architecture', 'infrastructure', 'career', 'data-and-messaging', 'quality-security']);
const fields = ['summary', 'details', 'points', 'sections', 'codeExamples', 'tables'];
const keyOf = q => q.normalize('NFC').trim();
// UUID v5, with the standard URL namespace and a dataset-specific name prefix.
function uuid(key) {
  const ns = Buffer.from('6ba7b8119dad11d180b400c04fd430c8', 'hex');
  const bytes = crypto.createHash('sha1').update(ns).update(`gdsc/interview-question/${key}`).digest().subarray(0, 16);
  bytes[6] = (bytes[6] & 15) | 80; bytes[8] = (bytes[8] & 63) | 128;
  const h = bytes.toString('hex');
  return `${h.slice(0,8)}-${h.slice(8,12)}-${h.slice(12,16)}-${h.slice(16,20)}-${h.slice(20)}`;
}
const slugify = q => q.normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/[đĐ]/g, 'd').toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '').slice(0, 100).replace(/-$/, '') || 'question';
const groups = new Map();
const datasets = new Map();
for (const file of files) {
  const rows = JSON.parse(fs.readFileSync(path.join(root, file), 'utf8'));
  assert(Array.isArray(rows)); datasets.set(file, rows);
  for (const row of rows) {
    assert.equal(typeof row.question, 'string'); assert(row.question.trim());
    for (const field of fields) assert(Array.isArray(row.answer[field]), `${file}: answer.${field}`);
    const key = keyOf(row.question);
    if (!groups.has(key)) groups.set(key, []);
    groups.get(key).push({ file, row });
  }
}
const canonical = [];
for (const [key, entries] of groups) {
  const existingIds = [...new Set(entries.map(e => e.row.id).filter(id => typeof id === 'string'))];
  assert(existingIds.length <= 1, 'Conflicting existing IDs');
  const id = existingIds[0] ?? uuid(key);
  const internalSlug = entries.find(e => e.row.legacyId !== undefined)?.row.slug ?? `${slugify(key)}-${id}`;
  const variants = new Map();
  for (const { file, row } of entries) {
    const departments = row.departments === undefined ? [missing[file]] : Array.isArray(row.departments) ? row.departments : [row.departments];
    const updated = { ...row, id, legacyId: row.legacyId ?? row.id, sourceSlug: row.sourceSlug !== undefined ? row.sourceSlug : row.slug, slug: internalSlug, departments: [...new Set(departments.map(d => aliases[d] ?? d))].sort() };
    assert(updated.departments.length && updated.departments.every(d => allowed.has(d)), `${file}: invalid department`);
    assert(Number.isInteger(updated.legacyId));
    if (check) assert.deepEqual(row, updated, `${file}: normalization required`);
    Object.assign(row, updated);
    const signature = JSON.stringify(row.answer);
    if (!variants.has(signature)) variants.set(signature, { answer: row.answer, sources: [] });
    variants.get(signature).sources.push({ file, legacyId: row.legacyId });
  }
  const first = entries[0].row;
  const answerVariants = [...variants.values()];
  const metadataConflict = ['level', 'access'].some(f => new Set(entries.map(e => e.row[f])).size > 1);
  canonical.push({
    id, question: first.question, slug: internalSlug,
    level: first.level, levelLabel: first.levelLabel, access: first.access,
    departments: [...new Set(entries.flatMap(e => e.row.departments))].sort(),
    topics: entries.map(e => path.basename(e.file, '.json')).sort(),
    answer: answerVariants.length === 1 ? first.answer : null,
    answerVariants,
    needsReview: answerVariants.length > 1 || metadataConflict,
    sources: entries.map(({ file, row }) => ({ file, legacyId: row.legacyId, sourceUrl: row.sourceUrl, sourceSlug: row.sourceSlug, level: row.level, levelLabel: row.levelLabel, access: row.access, upgrade: row.upgrade }))
  });
}
assert.equal(new Set(canonical.map(r => r.id)).size, canonical.length);
assert.equal(new Set(canonical.map(r => r.slug)).size, canonical.length);
for (const rows of datasets.values()) assert.equal(new Set(rows.map(r => r.legacyId)).size, rows.length);
const output = path.join(root, 'normalized');
const serialized = value => JSON.stringify(value, null, 2) + '\n';
const report = {
  files: files.length, sourceRecords: [...datasets.values()].reduce((n, a) => n + a.length, 0),
  uniqueQuestions: canonical.length, duplicateGroups: [...groups.values()].filter(a => a.length > 1).length,
  answerConflictGroups: canonical.filter(r => r.answerVariants.length > 1).length,
  needsReview: canonical.filter(r => r.needsReview).map(r => ({ id: r.id, question: r.question, sources: r.sources.map(s => ({ file: s.file, legacyId: s.legacyId })) }))
};
if (check) {
  assert.equal(fs.readFileSync(path.join(output, 'questions.json'), 'utf8'), serialized(canonical));
  assert.equal(fs.readFileSync(path.join(output, 'report.json'), 'utf8'), serialized(report));
} else {
  for (const [file, rows] of datasets) fs.writeFileSync(path.join(root, file), serialized(rows));
  fs.mkdirSync(output, { recursive: true });
  fs.writeFileSync(path.join(output, 'questions.json'), serialized(canonical));
  fs.writeFileSync(path.join(output, 'report.json'), serialized(report));
}
console.log(JSON.stringify({ ...report, needsReview: report.needsReview.length, check }, null, 2));
