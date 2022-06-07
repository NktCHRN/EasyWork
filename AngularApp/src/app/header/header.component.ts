import { Component, ElementRef, OnInit } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {

  showRows: boolean = false;

  isUserAuthenticated = (): boolean => {
    const token = localStorage.getItem("jwt");

    if (token && !this.jwtHelper.isTokenExpired(token)){
      return true;
    }

    return false;
  }
  
  logOut = () => {
    localStorage.removeItem("jwt");
  }

  constructor(private jwtHelper: JwtHelperService) { }

  ngOnInit(): void {  }

  showBtns() : void {
    this.showRows = !this.showRows;
  }
}
