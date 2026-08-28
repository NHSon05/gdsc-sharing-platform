# GDSC Sharing Platform - Design System & Component Instruction

> **Phiên bản:** 1.0.0  
> **Nền tảng:** Next.js 16 (App Router) + React 19 + Tailwind CSS v4 + shadcn/ui  
> **Typography chính:** Plus Jakarta Sans (Variable Font)  
> **Phong cách chủ đạo:** Minimalist Modern, Pill Geometry (`rounded-full`), High Contrast & Soft Ambient Elevation (Floating Glass).

---

## 1. Design Tokens & Nền tảng thiết kế (Design Foundations)

### 1.1. Typography

- **Font Family:** `Plus Jakarta Sans` (đặt tại `public/fonts/`).
- **Font Mono:** `Geist Mono` cho code snippets, API keys, tags kỹ thuật.
- **Quy chuẩn Font Weight:**
  - `Regular (400)`: Body copy, mô tả phụ, placeholder.
  - `Medium (500)`: Label input, button text (size sm/md), navigation links.
  - `SemiBold (600)`: Button text (size lg), tiêu đề card, subtitle.
  - `Bold (700)`: Headings (H1, H2, H3), số liệu thống kê nổi bật.

| Cấp bậc             | Font Size   | Line Height | Tracking | Weight           | Class Tailwind                                     |
| :------------------ | :---------- | :---------- | :------- | :--------------- | :------------------------------------------------- |
| **Display H1**      | 56px - 68px | 1.08        | -0.025em | Bold (700)       | `text-5xl md:text-[68px] font-bold tracking-tight` |
| **Heading H2**      | 32px - 36px | 1.15        | -0.02em  | Bold (700)       | `text-3xl md:text-4xl font-bold tracking-tight`    |
| **Heading H3**      | 20px - 24px | 1.25        | -0.015em | SemiBold (600)   | `text-xl md:text-2xl font-semibold tracking-tight` |
| **Body Large**      | 16px - 18px | 1.6         | normal   | Regular / Medium | `text-base md:text-lg leading-relaxed`             |
| **Body Base**       | 14px - 15px | 1.5         | normal   | Regular / Medium | `text-sm md:text-[15px] leading-normal`            |
| **Caption / Small** | 12px - 13px | 1.4         | +0.01em  | Medium / Regular | `text-xs md:text-[13px]`                           |

---

### 1.2. Bảng màu (Color Palette)

Hệ thống màu hỗ trợ đầy đủ 2 chế độ **Light Mode** và **Dark Mode** qua CSS Variables (OKLCH).

#### A. Neutral / Grayscale (Đen - Trắng - Xám)

- **Zing-950 (Black):** `#09090B` - Nền button Primary (Light), Chữ tiêu đề (Light), Nền Dark mode.
- **Zinc-900 / 800:** `#18181B` / `#27272A` - Card background (Dark), Hover states.
- **Zinc-400 / 500:** `#A1A1AA` / `#71717A` - Muted text, disabled text, borders.
- **Zinc-100 / 200:** `#F4F4F5` / `#E4E4E7` - Nền button Subtle, Outline border, Subtle backgrounds.
- **White:** `#FFFFFF` - Nền chính (Light), Chữ button Primary (Light).

#### B. Brand & Semantic Colors

- **Brand Primary Blue (Electric Azure):**
  - Base: `#3B82F6` (Tailwind `blue-500`)
  - Hover: `#2563EB` (Tailwind `blue-600`)
  - Glow Shadow: `rgba(59, 130, 246, 0.45)`
- **Community Accent Emerald (GDSC Signature):**
  - Base: `#10B981` (Tailwind `emerald-500`)
  - Hover: `#059669` (Tailwind `emerald-600`)
  - Soft Tint: `#ECFDF5` (`bg-emerald-50`)
- **Destructive Red:**
  - Base: `#EF4444` (Tailwind `red-500`)
  - Hover: `#DC2626` (Tailwind `red-600`)

---

### 1.3. Border Radius & Shadows (Độ bo góc & Đổ bóng)

- **Radius Tiêu chuẩn cho Interactive (Button, Tag, Input Pill):** `rounded-full` (Pill Geometry).
- **Radius cho Container (Card, Modal, Dialog):** `rounded-2xl` (16px) hoặc `rounded-3xl` (24px).
- **Shadow Tokens:**
  - **Flat / None:** `shadow-none`
  - **Subtle Card:** `shadow-[0_10px_30px_-10px_rgba(0,0,0,0.05)]`
  - **Brand Glow:** `shadow-[0_4px_14px_-2px_rgba(59,130,246,0.45)]`
  - **Elevated Soft Float (Glass Shadow):**  
    `shadow-[0_8px_24px_-4px_rgba(0,0,0,0.12),0_2px_6px_-1px_rgba(0,0,0,0.06)]`

---

## 2. Tiêu chuẩn Button Component (Button Design System)

Dựa trên bộ thiết kế 3 bảng (Standard Buttons, Icon-only Buttons, Elevated Floating Buttons), component `<Button />` được quy chuẩn hóa toàn diện:

### 2.1. Ma trận Variant (7 Biến thể)

| Tên Variant         | Mô tả giao diện                                                                             | Mục đích sử dụng                                                 |
| :------------------ | :------------------------------------------------------------------------------------------ | :--------------------------------------------------------------- |
| `primary` (default) | Nền đen đặc (`zinc-950`), chữ trắng, đảo màu ở dark mode.                                   | Nút kêu gọi hành động quan trọng nhất (Submit, Confirm, Create). |
| `outline`           | Nền trắng/trong suốt, viền xám mảnh (`border-zinc-200/90`), chữ tối.                        | Nút phụ cấp 2 (Cancel, Filter, Back, Secondary actions).         |
| `subtle`            | Nền xám nhạt (`zinc-100`), không viền, chữ xám đậm.                                         | Nút hành động nhẹ (Copy, More options, Category tags).           |
| `brand`             | Nền xanh dương rực rỡ (`blue-500`) kèm hiệu ứng đổ bóng phát sáng (Blue Glow).              | Nút điểm nhấn thương hiệu (Upgrade, Join Platform, Explore).     |
| `elevated`          | Nền trắng/xám tối nổi bật trên bề mặt nhờ lớp đổ bóng mềm 2 lớp (Soft Ambient Drop Shadow). | Nút floating, thẻ quick action trên dashboard hoặc hero section. |
| `ghost`             | Trong suốt hoàn toàn, chỉ hiện nền khi hover.                                               | Nút thanh công cụ, icon navigation trên navbar.                  |
| `destructive`       | Nền đỏ nổi bật (`red-500`), chữ trắng.                                                      | Thao tác nguy hiểm (Delete, Remove, Revoke access).              |

---

### 2.2. Kích thước (Size System)

| Size                   | Chiều cao (Height)    | Padding ngang | Font Size / Weight    | Kích thước Icon     |
| :--------------------- | :-------------------- | :------------ | :-------------------- | :------------------ |
| **`lg` (Large)**       | `48px` (`h-12`)       | `px-6`        | 15px / SemiBold (600) | `20px` (`size-5`)   |
| **`md` / `default`**   | `40px` (`h-10`)       | `px-5`        | 14px / Medium (500)   | `18px` (`size-4.5`) |
| **`sm` (Small)**       | `32px` (`h-8`)        | `px-3.5`      | 12px / Medium (500)   | `14px` (`size-3.5`) |
| **`icon-lg`**          | `48x48px` (`size-12`) | `p-0`         | -                     | `20px` (`size-5`)   |
| **`icon` / `icon-md`** | `40x40px` (`size-10`) | `p-0`         | -                     | `18px` (`size-4.5`) |
| **`icon-sm`**          | `32x32px` (`size-8`)  | `p-0`         | -                     | `14px` (`size-3.5`) |

---

### 2.3. Trạng thái tương tác (Interaction States)

Mỗi button bắt buộc phải phản hồi mượt mà qua 5 trạng thái:

1. **Rest (Mặc định):** Hiển thị đúng bảng màu và bóng định nghĩa.
2. **Hover:** Thay đổi nền nhẹ nhàng (`duration-200`), riêng variant `elevated` sẽ hơi nổi lên (`hover:-translate-y-0.5`).
3. **Active / Pressed:** Co nhẹ kích thước (`active:scale-[0.98]`), variant `elevated` giảm bóng xuống bề mặt.
4. **Focus-Visible:** Vòng sáng đôi (Double Focus Ring) chuẩn Accessibility với `focus-visible:ring-2 focus-visible:ring-offset-2`.
5. **Disabled:** Mờ dần (`disabled:opacity-60`), chặn con trỏ (`disabled:pointer-events-none disabled:cursor-not-allowed`).
6. **Loading:** Hiện spinner tự động, vô hiệu hóa click, ẩn icon để tránh giật layout.

---

## 3. Hướng dẫn Code & Sử dụng Component

Component đã được tích hợp sẵn tại: [`src/components/ui/button.tsx`](file:///Users/nguyenhongson/Documents/GDG/gdsc-sharing-platform/frontend/src/components/ui/button.tsx)

### 3.1. Các ví dụ sử dụng thông dụng

```tsx
import { Button } from "@/components/ui/button";
import { ArrowRight, Plus, Download, Sparkles, Trash2 } from "lucide-react";

export function ExamplePage() {
  return (
    <div className="flex flex-wrap items-center gap-4">
      {/* 1. Primary Button */}
      <Button variant="primary" size="md">
        Get Started
      </Button>

      {/* 2. Primary với Left & Right Icon */}
      <Button
        variant="primary"
        size="lg"
        leftIcon={<Sparkles />}
        rightIcon={<ArrowRight />}
      >
        Explore Roadmaps
      </Button>

      {/* 3. Brand Button (Blue Glow) */}
      <Button variant="brand" size="md" leftIcon={<Plus />}>
        Create Post
      </Button>

      {/* 4. Secondary Outline Button */}
      <Button variant="outline" size="md">
        Cancel
      </Button>

      {/* 5. Subtle Gray Button */}
      <Button variant="subtle" size="sm" leftIcon={<Download />}>
        Export Data
      </Button>

      {/* 6. Elevated Floating Soft Glass Button */}
      <Button variant="elevated" size="md">
        Floating Filter
      </Button>

      {/* 7. Destructive Button */}
      <Button variant="destructive" size="md" leftIcon={<Trash2 />}>
        Delete Account
      </Button>

      {/* 8. Circular Icon-only Buttons */}
      <Button variant="primary" size="icon" aria-label="Add new">
        <Plus />
      </Button>
      <Button variant="outline" size="icon-sm" aria-label="Download">
        <Download />
      </Button>
      <Button variant="brand" size="icon-lg" aria-label="Magic action">
        <Sparkles />
      </Button>

      {/* 9. Trạng thái Loading */}
      <Button variant="primary" size="md" loading>
        Saving Changes
      </Button>
    </div>
  );
}
```

---

## 4. Quy chuẩn mở rộng cho các Component tiếp theo

Để đảm bảo toàn bộ web GDSC Sharing Platform đồng bộ 100% với phong cách trên:

1. **Input / Form Control:**
   - Bo góc theo dạng `rounded-full` (với single input) hoặc `rounded-2xl` (với textarea/form container).
   - Viền `border-zinc-200/90 dark:border-zinc-800`, khi focus nhận `ring-2 ring-zinc-950 dark:ring-white`.
2. **Card & Bento Box:**
   - Dùng nền `bg-white dark:bg-zinc-900/80` kèm `backdrop-blur-md`.
   - Bo góc `rounded-3xl` (24px) với viền nhẹ `border border-zinc-100 dark:border-zinc-800/80`.
   - Hiệu ứng hover: `hover:-translate-y-1 hover:shadow-lg transition-all duration-300`.
3. **Modal & Dialog:**
   - Bọc trong lớp nền blur `bg-black/40 backdrop-blur-sm`.
   - Khung modal bo góc `rounded-3xl` với shadow `shadow-2xl`.
4. **Tag / Badge:**
   - Kế thừa cấu trúc pill `rounded-full`, padding `px-3 py-1`, font size `text-xs font-medium`.
