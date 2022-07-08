import { Component, Inject, OnInit } from '@angular/core';	
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { UsersService } from '../../services/users.service';
import { UserModel } from '../../shared/user/user.model';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  user: UserModel = undefined!;

  constructor(private _titleService: Title, private _route: ActivatedRoute, public usersService: UsersService, private _router: Router, 
    @Inject('projectName') private _projectName: string) { }

  ngOnInit(): void {
    this._route.paramMap.subscribe(params => {
      const id : number = parseInt(params.get('id')!);
      this.usersService.get(id)
      .subscribe({
        next: user => 
        {
          this.user = user;
          this._titleService.setTitle(`${this._projectName} - ${this.usersService.getFullName(user.firstName, user.lastName)}'s profile`);
        },
        error: () => this._router.navigate(["**"], { skipLocationChange: true })
      });
    });
  }

}
