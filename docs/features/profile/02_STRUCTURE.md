## 25. Backend Structure

### 25.1. Domain

```text
Domain/
├── Memberships/
│   ├── ClubGeneration
│   ├── ClubRole
│   ├── ClubMembership
│   ├── DepartmentMembership
│   └── RoleAssignment
└── Users/
    └── ApplicationUser profile rules where appropriate
```

Domain không phụ thuộc EF Core, Controller hoặc storage provider.

### 25.2. Application

```text
Application/Features/Profile/
├── Interfaces/
├── Models/
├── Validators/
└── Services/

Application/Features/Memberships/
├── Interfaces/
├── Models/
├── Validators/
└── Services/
```

### 25.3. Infrastructure

- EF configurations.
- Profile and membership service implementations.
- Migration và seeder.
- File storage.
- Email sender.
- Identity email-change integration.

### 25.4. API

- ProfileController.
- GenerationsController.
- DepartmentsController.
- ClubRolesController.
- AdminDepartmentsController.
- AdminGenerationsController.
- AdminMemberMembershipsController.

## 26. Frontend Structure

Frontend tuân theo:

```text
web/AGENTS.md
web/ARCHITECTURE.md
```

### 26.1. Profile feature

```text
web/src/features/profile/
├── api/
├── components/
├── hooks/
├── queries/
├── types/
└── index.ts
```

Component đề xuất:

```text
profile-header
profile-form
avatar-uploader
email-change-form
membership-history
generation-card
department-membership-card
role-badge-list
```

### 26.2. Admin membership feature

```text
web/src/features/member-management/
├── api/
├── components/
├── hooks/
├── queries/
├── types/
└── index.ts
```

Component đề xuất:

```text
generation-selector
department-multi-selector
role-multi-selector
membership-editor
department-management-dialog
```
