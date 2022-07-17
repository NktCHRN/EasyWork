import { NgxMatDateFormats } from "@angular-material-components/datetime-picker";

export const CUSTOM_DATE_FORMATS: NgxMatDateFormats = {
    parse: {
      dateInput: 'DD.MM.YYYY, HH:mm:ss'
    },
    display: {
      dateInput: 'DD.MM.YYYY, HH:mm:ss',
      monthYearLabel: 'MMM YYYY',
      dateA11yLabel: 'LL',
      monthYearA11yLabel: 'MMMM YYYY',
    }
  };
