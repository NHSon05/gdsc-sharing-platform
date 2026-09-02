# Hướng Dẫn Sử Dụng & Tài Liệu Props: Calendar & EventCalendar Components

Tài liệu chi tiết về cách sử dụng, kiểu dữ liệu (TypeScript Interfaces) và danh sách toàn bộ các `props` của các component trong bộ giải pháp Lịch & Sự kiện:

1. **`EventCalendar`**: Lịch sự kiện hoàn chỉnh (Google Calendar / Linear style) bao gồm Sidebar, Mini-calendar, Bộ lọc danh mục, Thanh chuyển chế độ xem (Day/Week/Month/Year), Lưới thời gian và Thẻ sự kiện.
2. **`Calendar`**: Bộ chọn ngày đơn lẻ (Single Date Picker) tích hợp sẵn chế độ chọn nhanh Tháng & Năm (Year/Month Picker).
3. **`DateRangePicker`**: Bộ chọn khoảng ngày với 2 tháng song song (Dual Month Grid) và thanh phím tắt (Presets Sidebar).
4. **`YearMonthPicker`**: Component chọn nhanh Tháng & Năm dạng cuộn/danh sách.

---

## 1. Component: `<EventCalendar />`

Component lịch biểu sự kiện toàn diện với giao diện 2 cột: Sidebar điều khiển bên trái và Lưới lịch chi tiết bên phải.

```tsx
import {
  EventCalendar,
  type CalendarEvent,
} from "@/components/ui/event-calendar";

const events: CalendarEvent[] = [
  {
    id: "1",
    title: "Council Chamber",
    startDate: new Date(2025, 11, 7, 1, 0),
    endDate: new Date(2025, 11, 7, 1, 45),
    location: "Location",
    color: "blue",
  },
  {
    id: "2",
    title: "Leadership Sync",
    startDate: new Date(2025, 11, 9, 2, 0),
    endDate: new Date(2025, 11, 9, 2, 50),
    location: "Meeting Mirage",
    color: "rose",
  },
];

export function MyScheduleView() {
  return (
    <EventCalendar
      date={new Date(2025, 11, 9)}
      events={events}
      onEventClick={(event) => console.log("Event clicked:", event)}
      onAddEvent={() => console.log("Open create event modal")}
      onSlotClick={(date, hour) => console.log("Slot clicked:", date, hour)}
    />
  );
}
```

### Danh sách Props của `EventCalendarProps`:

| Tên Prop           | Kiểu dữ liệu (Type)                      | Mặc định                     | Mô tả chi tiết                                                                               |
| :----------------- | :--------------------------------------- | :--------------------------- | :------------------------------------------------------------------------------------------- |
| `date`             | `Date`                                   | `new Date()`                 | Ngày đang được kích hoạt/hiển thị trên lịch.                                                 |
| `onDateChange`     | `(date: Date) => void`                   | `undefined`                  | Callback khi người dùng chọn ngày mới trên mini calendar hoặc bấm `<` `>` `Today`.           |
| `events`           | `CalendarEvent[]`                        | `[]`                         | Danh sách các sự kiện cần hiển thị trên lưới thời gian.                                      |
| `onEventClick`     | `(event: CalendarEvent) => void`         | `undefined`                  | Callback khi người dùng bấm vào một thẻ sự kiện (Event Card).                                |
| `onSlotClick`      | `(date: Date, hour: number) => void`     | `undefined`                  | Callback khi người dùng bấm vào một ô thời gian trống trên lưới lịch (để tạo sự kiện nhanh). |
| `onAddEvent`       | `() => void`                             | `undefined`                  | Callback khi người dùng bấm vào nút `+ Add event` ở Sidebar bên trái.                        |
| `viewMode`         | `'day' \| 'week' \| 'month' \| 'year'`   | `'week'`                     | Chế độ xem hiện tại của lịch.                                                                |
| `onViewModeChange` | `(mode: CalendarViewMode) => void`       | `undefined`                  | Callback khi người dùng chuyển đổi giữa các tab xem `Day`, `Week`, `Month`, `Year`.          |
| `categories`       | `CalendarCategory[]`                     | _Mặc định 4 mục_             | Danh sách các danh mục sự kiện trong bộ lọc Checkbox ở Sidebar.                              |
| `onCategoryToggle` | `(id: string, checked: boolean) => void` | `undefined`                  | Callback khi người dùng tick chọn hoặc bỏ chọn một danh mục sự kiện.                         |
| `accountEmail`     | `string`                                 | `"Hoangthuan0112@gmail.com"` | Tên tài khoản/Email hiển thị phía trên danh sách Checkbox ở Sidebar.                         |
| `timezone`         | `string`                                 | `"GMT +7"`                   | Nhãn hiển thị múi giờ ở góc trên bên trái của bảng giờ.                                      |
| `startHour`        | `number`                                 | `0`                          | Giờ bắt đầu hiển thị trên bảng lịch (từ `0` đến `23`).                                       |
| `endHour`          | `number`                                 | `12`                         | Giờ kết thúc hiển thị trên bảng lịch (mặc định hiển thị từ 00:00 đến 12:00).                 |
| `className`        | `string`                                 | `undefined`                  | Lớp CSS tùy chỉnh (Tailwind classes) cho khung ngoài cùng.                                   |

### Cấu trúc kiểu `CalendarEvent`:

```ts
export interface CalendarEvent {
  id: string;
  title: string;
  startDate: Date;
  endDate: Date;
  location?: string;
  color?: "blue" | "cyan" | "rose" | "amber" | "white" | "purple";
  category?: string;
  description?: string;
  allDay?: boolean;
}
```

---

## 2. Component: `<Calendar />` (Single Date Picker)

Component chọn ngày đơn lẻ với hỗ trợ chuyển đổi nhanh sang màn hình chọn Tháng & Năm.

```tsx
import { Calendar } from "@/components/ui/calendar";

export function SingleDatePickerDemo() {
  const [selectedDate, setSelectedDate] = React.useState<Date>(new Date());

  return (
    <Calendar
      value={selectedDate}
      onChange={setSelectedDate}
      showYearPicker={true}
    />
  );
}
```

### Danh sách Props của `CalendarProps`:

| Tên Prop         | Kiểu dữ liệu (Type)    | Mặc định     | Mô tả chi tiết                                                            |
| :--------------- | :--------------------- | :----------- | :------------------------------------------------------------------------ |
| `value`          | `Date`                 | `undefined`  | Ngày được chọn (Controlled mode).                                         |
| `defaultValue`   | `Date`                 | `new Date()` | Ngày được chọn ban đầu (Uncontrolled mode).                               |
| `onChange`       | `(date: Date) => void` | `undefined`  | Callback kích hoạt khi người dùng chọn một ngày mới.                      |
| `showYearPicker` | `boolean`              | `true`       | Cho phép bấm vào tiêu đề `Tháng Năm v` để mở bộ chọn Tháng & Năm nhanh.   |
| `minDate`        | `Date`                 | `undefined`  | Giới hạn ngày nhỏ nhất có thể chọn (các ngày trước đó sẽ bị vô hiệu hoá). |
| `maxDate`        | `Date`                 | `undefined`  | Giới hạn ngày lớn nhất có thể chọn.                                       |
| `className`      | `string`               | `undefined`  | Lớp CSS tùy chỉnh cho card lịch.                                          |

---

## 3. Component: `<DateRangePicker />` (Dual Month & Presets)

Component chọn khoảng ngày với 2 tháng liền kề và thanh danh sách phím tắt tiện lợi (Presets Sidebar).

```tsx
import { DateRangePicker, type DateRange } from "@/components/ui/calendar";

export function DateRangePickerDemo() {
  const [range, setRange] = React.useState<DateRange>({
    from: new Date(2025, 11, 11),
    to: new Date(2026, 0, 15),
  });

  return (
    <DateRangePicker value={range} onChange={setRange} showPresets={true} />
  );
}
```

### Danh sách Props của `DateRangePickerProps`:

| Tên Prop       | Kiểu dữ liệu (Type)                                   | Mặc định                   | Mô tả chi tiết                                                                     |
| :------------- | :---------------------------------------------------- | :------------------------- | :--------------------------------------------------------------------------------- |
| `value`        | `DateRange` (`{ from?: Date; to?: Date }`)            | `undefined`                | Khoảng ngày được chọn (Controlled mode).                                           |
| `defaultValue` | `DateRange`                                           | _7 ngày gần nhất_          | Khoảng ngày mặc định ban đầu.                                                      |
| `onChange`     | `(range: DateRange) => void`                          | `undefined`                | Callback khi khoảng ngày thay đổi (`from` và `to`).                                |
| `showPresets`  | `boolean`                                             | `true`                     | Bật/tắt thanh phím tắt (`1 Week ago`, `1 Month ago`, `1 Year ago`, v.v.) bên phải. |
| `presets`      | `Array<{ label: string; getValue: () => DateRange }>` | _Danh sách 5 mốc mặc định_ | Tùy biến danh sách các phím tắt chọn nhanh theo nghiệp vụ riêng.                   |
| `className`    | `string`                                              | `undefined`                | Lớp CSS tùy chỉnh cho container.                                                   |

---

## 4. Bảng mã màu sự kiện (`EventColor`):

| Màu           | Giá trị `color` | Bối cảnh sử dụng phù hợp                                                   |
| :------------ | :-------------- | :------------------------------------------------------------------------- |
| 🔵 **Blue**   | `"blue"`        | Các buổi họp hội đồng, ban chủ nhiệm (`Council Chamber`, `Daily Standup`). |
| 🟢 **Cyan**   | `"cyan"`        | Sự kiện hội thảo, sinh hoạt câu lạc bộ (`Event Name`, `Leadership Sync`).  |
| 🔴 **Rose**   | `"rose"`        | Demo sản phẩm, thảo luận 1-on-1 quan trọng (`Product Demo`, `1-on-1`).     |
| 🟡 **Amber**  | `"amber"`       | Các buổi workshop kéo dài nhiều giờ liên tục.                              |
| ⚪ **White**  | `"white"`       | Kiểm thử người dùng, phỏng vấn, sự kiện thông thường (`Usability Test`).   |
| 🟣 **Purple** | `"purple"`      | Hoạt động giải trí, team building, hackathon.                              |
