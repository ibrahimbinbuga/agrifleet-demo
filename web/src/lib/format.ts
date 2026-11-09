const dateTimeFormat = new Intl.DateTimeFormat('tr-TR', {
  dateStyle: 'short',
  timeStyle: 'short',
  timeZone: 'Europe/Istanbul',
});

const timeFormat = new Intl.DateTimeFormat('tr-TR', { timeStyle: 'medium', timeZone: 'Europe/Istanbul' });

export const formatDateTime = (value: string): string => dateTimeFormat.format(new Date(value));

export const formatTime = (value: string): string => timeFormat.format(new Date(value));

export const formatNumber = (value: number, fractionDigits = 0): string =>
  value.toLocaleString('tr-TR', { minimumFractionDigits: fractionDigits, maximumFractionDigits: fractionDigits });

export const formatHours = (minutes: number): string => `${formatNumber(minutes / 60, 1)} sa`;
