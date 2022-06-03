import { Component, ElementRef, OnInit } from '@angular/core';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {

  showRows: boolean = false;

  constructor() { }

  ngOnInit(): void {  }

  showBtns() : void {
    this.showRows = !this.showRows;
  }
}
