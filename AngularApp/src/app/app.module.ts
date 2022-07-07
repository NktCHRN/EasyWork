import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { MatToolbarModule } from '@angular/material/toolbar'; 
import {MatIconModule} from '@angular/material/icon';
import { HeaderComponent } from './components/header/header.component';
import { MatSidenavModule } from '@angular/material/sidenav';
import { FlexLayoutModule } from '@angular/flex-layout';
import {MatDividerModule} from '@angular/material/divider'; 
import {MatButtonModule} from '@angular/material/button'
import {MatMenuModule} from '@angular/material/menu';
import { FooterComponent } from './components/footer/footer.component';
import { HomeComponent } from './components/home/home.component'
import { baseURL } from './shared/constants/baseurl';
import { ProcessHTTPMsgService } from './services/process-httpmsg.service';
import { GeneralinfoService } from './services/generalinfo.service';
import { HttpClientModule } from '@angular/common/http';
import { NotfoundComponent } from './components/notfound/notfound.component';
import { RegistrationComponent } from './components/registration/registration.component';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {MatNativeDateModule} from '@angular/material/core';
import {MatFormFieldModule} from '@angular/material/form-field'; 
import { MatInputModule } from '@angular/material/input';
import { AccountService } from './services/account.service';
import { appURL } from './shared/constants/app-url';
import {MatSnackBarModule} from '@angular/material/snack-bar';
import { EmailConfirmationComponent } from './components/email-confirmation/email-confirmation.component';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner'; 
import { JwtModule } from "@auth0/angular-jwt";
import { LoginComponent } from './components/login/login.component';
import { CabinetComponent } from './components/cabinet/cabinet.component';
import { SocialLoginModule, SocialAuthServiceConfig } from 'angularx-social-login';
import { GoogleLoginProvider } from 'angularx-social-login';
import { UserinfoService } from './services/userinfo.service';
import { TokenGuardService } from './services/token-guard.service';
import {MatBadgeModule} from '@angular/material/badge';
import { ResetPasswordComponent } from './components/reset-password/reset-password.component';
import { emailConfirmationURL } from './shared/constants/email-confirmation-url';
import { AvatarShowComponent } from './components/cabinet/avatar/avatar-show/avatar-show.component';
import { AvatarEditComponent } from './components/cabinet/avatar/avatar-edit/avatar-edit.component';
import {MatDialogModule} from '@angular/material/dialog';
import { AvatarBaseComponent } from './components/cabinet/avatar/avatar-base/avatar-base.component';
import { AvatarDeleteComponent } from './components/cabinet/avatar/avatar-delete/avatar-delete.component'; 
import { ImageCropperModule } from 'ngx-image-cropper';
import { ErrorDialogComponent } from './components/error-dialog/error-dialog.component';
import { authInterceptor } from './interceptors/auth.interceptor';
import { ProfileComponent } from './components/profile/profile.component';
import { UsersService } from './services/users.service';
import { TasksComponent } from './components/tasks/tasks.component';
import { TasksService } from './services/tasks.service';
import {MatCardModule} from '@angular/material/card';
import { BannedComponent } from './components/banned/banned.component'; 
import { ProjectsService } from './services/projects.service';
import { ProjectsComponent } from './components/projects/projects.component';
import { ProjectAddComponent } from './components/projects/project-add/project-add.component';
import {MatButtonToggleModule} from '@angular/material/button-toggle';
import { ProjectJoinComponent } from './components/projects/project-add/project-join/project-join.component';
import { ProjectCreateComponent } from './components/projects/project-add/project-create/project-create.component';
import { ProjectComponent } from './components/project/project.component'; 
import { InvitesService } from './services/invites.service';
import { InviteComponent } from './components/invite/invite.component';

export function tokenGetter() { 
  return localStorage.getItem("jwt"); 
}

@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    FooterComponent,
    HomeComponent,
    NotfoundComponent,
    RegistrationComponent,
    EmailConfirmationComponent,
    LoginComponent,
    CabinetComponent,
    ResetPasswordComponent,
    AvatarShowComponent,
    AvatarEditComponent,
    AvatarBaseComponent,
    AvatarDeleteComponent,
    ErrorDialogComponent,
    ProfileComponent,
    TasksComponent,
    BannedComponent,
    ProjectsComponent,
    ProjectAddComponent,
    ProjectJoinComponent,
    ProjectCreateComponent,
    ProjectComponent,
    InviteComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    MatToolbarModule,
    MatIconModule,
    MatSidenavModule,
    FlexLayoutModule,
    MatMenuModule,
    MatButtonModule,
    MatDividerModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    JwtModule.forRoot({
      config: {
        tokenGetter: tokenGetter,
        allowedDomains: ["localhost:7255"],
        disallowedRoutes: []
      }
    }),
    SocialLoginModule,
    MatBadgeModule,
    MatDialogModule,
    ImageCropperModule,
    MatCardModule,
    MatButtonToggleModule
  ],
  providers: [
    GeneralinfoService,
    ProcessHTTPMsgService,
    {provide: 'baseURL', useValue: baseURL},
    {provide: 'appURL', useValue: appURL},
    {provide: 'emailConfirmationURL', useValue: emailConfirmationURL},
    AccountService,
    {
      provide: 'SocialAuthServiceConfig',
      useValue: {
        autoLogin: false,
        providers: [
          {
            id: GoogleLoginProvider.PROVIDER_ID,
            provider: new GoogleLoginProvider(
              '133068226796-q4hbsusi3uqpeqj3s1tlii1ifknqc2s3.apps.googleusercontent.com'
            )
          },
        ],
      } as SocialAuthServiceConfig
    },
    UserinfoService,
    TokenGuardService,
    authInterceptor,
    UsersService,
    TasksService,
    ProjectsService,
    InvitesService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
