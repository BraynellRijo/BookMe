import { ChangeDetectionStrategy, Component, model } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';

@Component({
  selector: 'app-booking-calendar',
  imports: [MatCardModule, MatDatepickerModule],
  providers: [provideNativeDateAdapter()],
  templateUrl: './booking-calendar.html',
  styleUrl: './booking-calendar.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookingCalendar {
  selected = model<Date | null>(null);

}
