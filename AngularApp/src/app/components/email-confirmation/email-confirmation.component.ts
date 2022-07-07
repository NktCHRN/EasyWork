import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, Inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute } from '@angular/router';
import { AccountService } from '../../services/account.service';
import { ResendEmailConfirmation } from '../../shared/user/resendemailconfirmation';

@Component({
  selector: 'app-email-confirmation',
  templateUrl: './email-confirmation.component.html',
  styleUrls: ['./email-confirmation.component.scss']
})
export class EmailConfirmationComponent implements OnInit {
  showSuccess: boolean = false;
  errorMessage: string | undefined | null;
  disableButton: boolean = false;

  constructor(private _accountService: AccountService, private _route: ActivatedRoute, 
    @Inject('emailConfirmationURL') private _emailConfirmationURL: string,
    private _snackBar: MatSnackBar) { }

  ngOnInit(): void {
    this.confirmEmail();
  }

  private confirmEmail = () => {
    const token = this._route.snapshot.queryParams['token'];
    const email = this._route.snapshot.queryParams['email'];
    
    this._accountService.confirmEmail(token, email)
    .subscribe({
      next: () => this.showSuccess = true,
      error: (err: HttpErrorResponse) => {
        this.errorMessage = err.error ?? err.message ?? err;
      }
    })
  }

  public resendEmail()
  {
    this.disableButton = true;
    let model = new ResendEmailConfirmation;
    model.email = this._route.snapshot.queryParams['email'],
    model.clientURI = this._emailConfirmationURL;
    this._accountService.resendEmail(model)
    .subscribe({
      next: () => 
      {
        this._snackBar.open("The email has been sent once more successfully", "Close", {duration: 5000})
      },
      error: err => {
        this._snackBar.open("Error: " + err.error, "Close")
      }
    })
  }
}
