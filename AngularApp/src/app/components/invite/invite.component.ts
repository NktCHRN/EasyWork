import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { InvitesService } from 'src/app/services/invites.service';
import { TokenService } from 'src/app/services/token.service';

@Component({
  selector: 'app-invite',
  templateUrl: './invite.component.html',
  styleUrls: ['./invite.component.scss']
})
export class InviteComponent implements OnInit {
  loading: boolean = true;
  errorMessage: string | null | undefined;

  constructor(private _route: ActivatedRoute, private _invitesService: InvitesService, private _router: Router, private _tokenService: TokenService) { }

  ngOnInit(): void {
    this._route.paramMap.subscribe(params => {
      const code : string = params.get('code')!;
      this._invitesService.join(this._tokenService.getJwtToken()!, code)
      .subscribe({
        next: result => {
          this._router.navigate(['projects', result.projectId]);
        },
        error: error => {
          this.loading = false;
          this.errorMessage = error?.error ?? JSON.stringify(error);
        }
      });
    });
  }

}
