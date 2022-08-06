import { Component, Input, OnInit } from '@angular/core';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { UserProfileReducedModel } from 'src/app/shared/user/user-profile-reduced.model';

@Component({
  selector: 'app-admin-user',
  templateUrl: './admin-user.component.html',
  styleUrls: ['./admin-user.component.scss']
})
export class AdminUserComponent implements OnInit {
  @Input() user: UserProfileReducedModel = undefined!;
  @Input() connectionContainer: ConnectionContainer = undefined!;

  constructor() { }

  ngOnInit(): void {
  }

}
