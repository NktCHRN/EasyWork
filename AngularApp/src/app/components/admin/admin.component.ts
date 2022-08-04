import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { createNotWhitespaceValidator } from 'src/app/customvalidators';
import { UserService } from 'src/app/services/user.service';
import { UserProfileReducedModel } from 'src/app/shared/user/user-profile-reduced.model';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss']
})
export class AdminComponent implements OnInit {
  form: FormGroup = null!;
  @ViewChild('sform') loginFormDirective: any;
  formError: string | null | undefined;

  private readonly searchFormErrorMessage = 'Please, type at least three characters into the form above'; 

  search: FormControl = undefined!;

  searchOldValue: string | null | undefined;

  users: UserProfileReducedModel[] = undefined!

  loading: boolean = false;
  errorMessage: string | null | undefined;

  readonly minLength = 3;

  constructor(private _fb: FormBuilder, private _router: Router, private _route: ActivatedRoute, private _userService: UserService) {
    this.createForm();
   }

  ngOnInit(): void {
    this.searchOldValue = this._route.snapshot.queryParams['search'];
    if (this.searchOldValue && this.searchOldValue.length >= this.minLength)
      this.getResults();
  }

  private getResults(): void
  {
    this.errorMessage = null;
    this.loading = true;
    this._userService.get(this.searchOldValue)
    .subscribe({
      next: result =>
      {
        this.loading = false;
        this.users = result;
      },
      error: error =>
      {
        this.loading = false;
        const message = `An error occured: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
        this.errorMessage = message;
        console.error(message);
      }
    });
  }

  createForm() {
    this.search = new FormControl('', [Validators.required, createNotWhitespaceValidator(), Validators.minLength(3)]);
    this.form = this._fb.group({
      search: this.search
    });
  }

  public onSubmit(): void
  {
    if (!this.form.valid)
    {
      this.formError = this.searchFormErrorMessage;
      return;
    }
    this.formError = null;
    this.searchOldValue = this.search.value;
    this._router.navigate(['/admin'], { queryParams: { search: this.searchOldValue } });
    this.getResults();
  }
}
