import { Component, Input, OnInit } from '@angular/core';
import { TokenService } from 'src/app/services/token.service';
import { UserProfileReducedModel } from 'src/app/shared/user/user-profile-reduced.model';

@Component({
  selector: 'app-admin-user',
  templateUrl: './admin-user.component.html',
  styleUrls: ['./admin-user.component.scss']
})
export class AdminUserComponent implements OnInit {
  @Input() user: UserProfileReducedModel = undefined!;

  constructor() { }

  ngOnInit(): void {
  }

}
