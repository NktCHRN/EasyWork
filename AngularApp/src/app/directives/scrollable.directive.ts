import { Directive, ElementRef, HostListener, Input } from '@angular/core';

@Directive({
  selector: '[appScrollable]',
  exportAs: "appScrollable"
})
export class ScrollableDirective {
  constructor(private _elementRef: ElementRef) {}

  @Input() scrollUnit: number = 100;

  private _scrollLeft: number = 0;

  private get element() {
    return this._elementRef.nativeElement;
  }

  get isOverflow() {
    return this.element.scrollWidth > this.element.clientWidth;
  }

  scroll(direction: number) {
    this._scrollLeft = this.element.scrollLeft + this.scrollUnit * direction;
    if (this._scrollLeft < 0)
      this._scrollLeft = 0;
    else if (this._scrollLeft > this.element.scrollWidth - this.element.clientWidth)
      this._scrollLeft = this.element.scrollWidth - this.element.clientWidth;
    this.element.scrollTo({
      left: this.element.scrollLeft + this.scrollUnit * direction,
      behavior: 'smooth'
    });
  }

  get canScrollStart() {
    return this._scrollLeft > 0;
  }

  get canScrollEnd() {
    return this._scrollLeft + this.element.clientWidth != this.element.scrollWidth;
  }

  @HostListener("window:resize")
  onWindowResize() {
    this._scrollLeft = this.element.scrollLeft;
  } // required for update view when windows resized
}
