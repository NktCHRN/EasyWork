import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'fileSize'
})
export class FileSizePipe implements PipeTransform {

  transform(value: number): string {
    const suffixes: string[] = [ "B", "KB", "MB", "GB" ];
    let i;
    let dblSByte: number = value;
    for (i = 0; i < suffixes.length && value >= 1024; i++, value /= 1024)
        dblSByte = value / 1024.0;
    let result: string;
    const stringified: string = dblSByte.toString();
    if (dblSByte.toFixed() == stringified || dblSByte.toFixed(1) == stringified)
      result = stringified;
    else
      result = dblSByte.toFixed(2);
    result += ` ${suffixes[i]}`;
    return result;
  }

}
