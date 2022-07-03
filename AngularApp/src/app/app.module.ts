import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { MatToolbarModule } from '@angular/material/toolbar'; 
import {MatIconModule} from '@angular/material/icon';
import { HeaderComponent } from './header/header.component';
import { MatSidenavModule } from '@angular/material/sidenav';
import { FlexLayoutModule } from '@angular/flex-layout';
import {MatDividerModule} from '@angular/material/divider'; 
import {MatButtonModule} from '@angular/material/button'
import {MatMenuModule} from '@angular/material/menu';
import { FooterComponent } from './footer/footer.component';
import { HomeComponent } from './home/home.component'
import { baseURL } from './shared/baseurl';
import { ProcessHTTPMsgService } from './services/process-httpmsg.service';
import { GeneralinfoService } from './services/generalinfo.service';
import { HttpClientModule } from '@angular/common/http';
import { NotfoundComponent } from './notfound/notfound.component';
import { RegistrationComponent } from './registration/registration.component';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {MatNativeDateModule} from '@angular/material/core';
import {MatFormFieldModule} from '@angular/material/form-field'; 
import { MatInputModule } from '@angular/material/input';
import { AccountService } from './services/account.service';
import { appURL } from './shared/app-url';
import {MatSnackBarModule} from '@angular/material/snack-bar';
import { EmailConfirmationComponent } from './email-confirmation/email-confirmation.component';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner'; 
import { JwtModule } from "@auth0/angular-jwt";
import { LoginComponent } from './login/login.component';
import { CabinetComponent } from './cabinet/cabinet.component';
import { SocialLoginModule, SocialAuthServiceConfig } from 'angularx-social-login';
import { GoogleLoginProvider } from 'angularx-social-login';
import { UserinfoService } from './services/userinfo.service';
import { TokenGuardService } from './services/token-guard.service';
import {MatBadgeModule} from '@angular/material/badge';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { emailConfirmationURL } from './shared/email-confirmation-url';

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
    MatBadgeModule
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
    TokenGuardService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
