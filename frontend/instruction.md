# GDSC Sharing Platform - Design System & Component Instruction

> **Phiên bản:** 1.1.0  
> **Nền tảng:** Next.js 16 (App Router) + React 19 + Tailwind CSS v4 + shadcn/ui  
> **Typography chính:** Plus Jakarta Sans (Variable Font)  
> **Màu chủ đạo (Primary Brand):** Google / GDSC Blue `#4285F4`  
> **Phong cách chủ đạo:** Minimalist Modern, Pill Geometry (`rounded-full`), High Contrast & Soft Ambient Elevation (Floating Glass).

---

## 1. Design Tokens & Biến màu tái sử dụng (Reusable Color Variables)

Hệ thống màu được cấu hình thành CSS Variables trong [`src/app/globals.css`](file:///Users/nguyenhongson/Documents/GDG/gdsc-sharing-platform/frontend/src/app/globals.css) và tự động nhận diện thành class Tailwind CSS tiện lợi:

### 1.1. Bảng màu GDSC Brand Tokens

| Token CSS Variable | Giá trị Hex (Light) | Giá trị Hex (Dark) | Class Tailwind CSS | Mục đích sử dụng |
| :--- | :--- | :--- | :--- | :--- |
| `--brand` | `#4285F4` | `#4285F4` | `bg-brand`, `text-brand`, `border-brand` | Màu chính thức của GDSC (Google Blue) |
| `--brand-hover` | `#3367D6` | `#5C9BF6` | `hover:bg-brand-hover` | Trạng thái hover của nút/link brand |
| `--brand-muted` | `#E8F0FE` | `rgba(66,133,244,0.15)` | `bg-brand-muted` | Nền icon badge, chip, tag nhẹ |
| `--brand-border` | `#D2E3FC` | `rgba(66,133,244,0.35)` | `border-brand-border` | Viền của thẻ nổi, pill badge |
| `--brand-glow` | `rgba(66,133,244,0.45)` | `rgba(66,133,244,0.6)` | `shadow-[0_4px_14px_-2px_var(--brand-glow)]` | Hiệu ứng phát sáng neon / ambient glow |

#### Bộ màu mở rộng Google Developer Groups (GDSC Core Palette)
* **GDSC Blue:** `--gdsc-blue: #4285F4` ➔ `text-gdsc-blue` / `bg-gdsc-blue`
* **GDSC Red:** `--gdsc-red: #EA4335` ➔ `text-gdsc-red` / `bg-gdsc-red`
* **GDSC Yellow:** `--gdsc-yellow: #FBBC04` ➔ `text-gdsc-yellow` / `bg-gdsc-yellow`
* **GDSC Green:** `--gdsc-green: #34A853` ➔ `text-gdsc-green` / `bg-gdsc-green`

---

### 1.2. Typography
* **Font Family:** `Plus Jakarta Sans` (đặt tại `public/fonts/`).
* **Font Mono:** `Geist Mono` cho code snippets, API keys, tags kỹ thuật.
* **Quy chuẩn Font Weight:**
  - `Regular (400)`: Body copy, mô tả phụ, placeholder.
  - `Medium (500)`: Label input, button text (size sm/md), navigation links.
  - `SemiBold (600)`: Button text (size lg), tiêu đề card, subtitle.
  - `Bold (700)`: Headings (H1, H2, H3), số liệu thống kê nổi bật.

| Cấp bậc | Font Size | Line Height | Tracking | Weight | Class Tailwind |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Display H1** | 56px - 68px | 1.08 | -0.025em | Bold (700) | `text-5xl md:text-[68px] font-bold tracking-tight text-brand` |
| **Heading H2** | 32px - 36px | 1.15 | -0.02em | Bold (700) | `text-3xl md:text-4xl font-bold tracking-tight` |
| **Heading H3** | 20px - 24px | 1.25 | -0.015em | SemiBold (600) | `text-xl md:text-2xl font-semibold tracking-tight` |
| **Body Large** | 16px - 18px | 1.6 | normal | Regular / Medium | `text-base md:text-lg leading-relaxed` |
| **Body Base** | 14px - 15px | 1.5 | normal | Regular / Medium | `text-sm md:text-[15px] leading-normal` |
| **Caption / Small** | 12px - 13px | 1.4 | +0.01em | Medium / Regular | `text-xs md:text-[13px]` |

---

### 1.3. Border Radius & Shadows
* **Radius cho Interactive (Button, Tag, Input Pill):** `rounded-full` (Pill Geometry).
* **Radius cho Container (Card, Modal, Dialog):** `rounded-2xl` (16px) hoặc `rounded-3xl` (24px).
* **Shadow Tokens:**
  - **Flat / None:** `shadow-none`
  - **Subtle Card:** `shadow-[0_10px_30px_-10px_rgba(0,0,0,0.05)]`
  - **Brand Glow:** `shadow-[0_4px_14px_-2px_var(--brand-glow)]`
  - **Elevated Soft Float (Glass Shadow):**  
    `shadow-[0_8px_24px_-4px_rgba(0,0,0,0.12),0_2px_6px_-1px_rgba(0,0,0,0.06)]`

---

## 2. Tiêu chuẩn Button Component (Button Design System)

Dựa trên bộ thiết kế 3 bảng (Standard Buttons, Icon-only Buttons, Elevated Floating Buttons), component `<Button />` được quy chuẩn hóa toàn diện:

### 2.1. Ma trận Variant (7 Biến thể)

| Tên Variant | Mô tả giao diện | Mục đích sử dụng |
| :--- | :--- | :--- |
| `primary` (default) | Nền đen đặc (`zinc-950`), chữ trắng, đảo màu ở dark mode. | Nút kêu gọi hành động chính thức (Submit, Confirm, Create). |
| `brand` | Nền xanh GDSC Blue (`#4285F4`) kèm hiệu ứng phát sáng Blue Glow. | Nút điểm nhấn thương hiệu GDSC (Login, Join Platform, Explore). |
| `outline` | Nền trắng/trong suốt, viền xám mảnh (`border-zinc-200/90`), chữ tối. | Nút phụ cấp 2 (Cancel, Filter, Back, Secondary actions). |
| `subtle` | Nền xám nhạt (`zinc-100`), không viền, chữ xám đậm. | Nút hành động nhẹ (Copy, More options, Category tags). |
| `elevated` | Nền trắng/xám tối nổi bật trên bề mặt nhờ lớp đổ bóng mềm 2 lớp (Soft Ambient Drop Shadow). | Nút floating, thẻ quick action trên dashboard hoặc hero section. |
| `ghost` | Trong suốt hoàn toàn, chỉ hiện nền khi hover. | Nút thanh công cụ, icon navigation trên navbar. |
| `destructive` | Nền đỏ nổi bật (`red-500`), chữ trắng. | Thao tác nguy hiểm (Delete, Remove, Revoke access). |

---

### 2.2. Kích thước (Size System)

| Size | Chiều cao (Height) | Padding ngang | Font Size / Weight | Kích thước Icon |
| :--- | :--- | :--- | :--- | :--- |
| **`lg` (Large)** | `48px` (`h-12`) | `px-6` | 15px / SemiBold (600) | `20px` (`size-5`) |
| **`md` / `default`** | `40px` (`h-10`) | `px-5` | 14px / Medium (500) | `18px` (`size-4.5`) |
| **`sm` (Small)** | `32px` (`h-8`) | `px-3.5` | 12px / Medium (500) | `14px` (`size-3.5`) |
| **`icon-lg`** | `48x48px` (`size-12`) | `p-0` | - | `20px` (`size-5`) |
| **`icon` / `icon-md`** | `40x40px` (`size-10`) | `p-0` | - | `18px` (`size-4.5`) |
| **`icon-sm`** | `32x32px` (`size-8`) | `p-0` | - | `14px` (`size-3.5`) |

---

### 2.3. Hướng dẫn sử dụng trong React/Next.js

```tsx
import { Button } from "@/components/ui/button";
import { Sparkles, ArrowRight, Plus, Download, Trash2 } from "lucide-react";

export function ExampleUsage() {
  return (
    <div className="flex flex-wrap gap-4 items-center p-6">
      {/* 1. Brand Button GDSC Blue */}
      <Button variant="brand" size="lg" leftIcon={<Sparkles />} rightIcon={<ArrowRight />}>
        Khám phá GDSC Roadmaps
      </Button>

      {/* 2. Primary Button (Đen/Trắng tối giản) */}
      <Button variant="primary" size="md">
        Xác nhận
      </Button>

      {/* 3. Elevated Floating Glass Button */}
      <Button variant="elevated" size="md" className="text-brand">
        Xem chi tiết
      </Button>

      {/* 4. Subtle Button */}
      <Button variant="subtle" size="sm" leftIcon={<Download />}>
        Tải tài liệu
      </Button>

      {/* 5. Circular Icon-only Buttons */}
      <Button variant="brand" size="icon" aria-label="Thêm mới">
        <Plus />
      </Button>
    </div>
  );
}
```

---

## 3. Cách tái sử dụng Color Variables trong các Component tùy chỉnh

Khi tạo component mới (Card, Badge, Input, Sidebar, Modal), bạn có thể gọi trực tiếp các biến Tailwind CSS đã đăng ký:

```html
<!-- Badge trạng thái GDSC Blue -->
<div class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-brand-muted border border-brand-border text-brand text-xs font-semibold">
  <span>GDSC Member</span>
</div>

<!-- Card viền hover đổi sang GDSC Blue -->
<div class="rounded-3xl bg-white dark:bg-zinc-900 p-6 border border-zinc-100 dark:border-zinc-800 hover:border-brand/60 hover:shadow-[0_20px_40px_-15px_var(--brand-glow)] transition-all duration-300">
  <h3 class="text-xl font-bold hover:text-brand">Tiêu đề bài viết</h3>
</div>
```
