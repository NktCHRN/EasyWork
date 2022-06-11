import { Component, OnInit } from '@angular/core';
import { TokenGuardService } from '../services/token-guard.service';

@Component({
  selector: 'app-notfound',
  templateUrl: './notfound.component.html',
  styleUrls: ['./notfound.component.scss']
})
export class NotfoundComponent implements OnInit {

  constructor(private tokenGuardService: TokenGuardService) { }

  ngOnInit(): void {
    this.tokenGuardService.refreshToken();
  }

}
