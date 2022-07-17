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
import {MatNativeDateModule, MAT_DATE_LOCALE} from '@angular/material/core';
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
import { UserInfoService } from './services/userinfo.service';
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
import { UserService } from './services/user.service';
import { TasksComponent } from './components/tasks/tasks.component';
import { TaskService } from './services/task.service';
import {MatCardModule} from '@angular/material/card';
import { BannedComponent } from './components/banned/banned.component'; 
import { ProjectService } from './services/project.service';
import { ProjectsComponent } from './components/projects/projects.component';
import { ProjectAddComponent } from './components/projects/project-add/project-add.component';
import {MatButtonToggleModule} from '@angular/material/button-toggle';
import { ProjectJoinComponent } from './components/projects/project-add/project-join/project-join.component';
import { ProjectCreateComponent } from './components/projects/project-add/project-create/project-create.component';
import { ProjectComponent } from './components/project/project.component'; 
import { InviteService } from './services/invite.service';
import { InviteComponent } from './components/invite/invite.component';
import { ProjectInfoComponent } from './components/project/project-info/project-info.component';
import { ProjectTasksComponent } from './components/project/project-tasks/project-tasks.component';
import { ProjectArchiveComponent } from './components/project/project-archive/project-archive.component';
import { ProjectGanttComponent } from './components/project/project-gantt/project-gantt.component';
import { projectName } from './shared/constants/project-name';
import { ProjectParticipantsComponent } from './components/project/project-participants/project-participants.component';
import { ProjectInfoShowComponent } from './components/project/project-info/project-info-show/project-info-show.component';
import { ProjectInfoEditComponent } from './components/project/project-info/project-info-edit/project-info-edit.component';
import { ProjectInfoDeleteComponent } from './components/project/project-info/project-info-delete/project-info-delete.component';
import { ProjectInviteComponent } from './components/project/invite/project-invite/project-invite.component';
import { MatSlideToggleModule} from '@angular/material/slide-toggle'; 
import {ClipboardModule} from '@angular/cdk/clipboard';
import { ProjectInviteRegenerateComponent } from './components/project/invite/project-invite-regenerate/project-invite-regenerate.component'; 
import {MatTableModule} from '@angular/material/table'; 
import {MatTooltipModule} from '@angular/material/tooltip'; 
import { ProjectRoleService } from './services/project-role.service';
import { ProjectLeaveComponent } from './components/project/project-participants/project-leave/project-leave.component';
import { ProjectKickComponent } from './components/project/project-participants/project-kick/project-kick.component';
import { ProjectUserEditComponent } from './components/project/project-participants/project-user-edit/project-user-edit.component';
import {MatSelectModule} from '@angular/material/select';
import { ProjectUserAddComponent } from './components/project/project-participants/project-user-add/project-user-add.component'; 
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import {MatExpansionModule} from '@angular/material/expansion';
import { TaskReducedComponent } from './components/project/project-tasks/task-reduced/task-reduced.component';
import { ScrollableDirective } from './directives/scrollable.directive'; 
import {MatChipsModule} from '@angular/material/chips';
import { ProjectTagDeleteComponent } from './components/project/project-tasks/project-tag-delete/project-tag-delete.component';
import { TaskAddComponent } from './components/project/project-tasks/task-add/task-add.component';
import { TaskComponent } from './components/project/project-tasks/task/task.component'; 
import {
  NgxMatDateAdapter,
  NgxMatDatetimePickerModule, 
  NgxMatNativeDateModule, 
  NgxMatTimepickerModule, 
  NGX_MAT_DATE_FORMATS
} from '@angular-material-components/datetime-picker';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { CustomNgxDatetimeAdapter } from './shared/other/custom-date-adapter';
import { NGX_MAT_MOMENT_DATE_ADAPTER_OPTIONS } from '@angular-material-components/moment-adapter';
import { CUSTOM_DATE_FORMATS} from './shared/constants/date-formats';
import { TaskDeleteComponent } from './components/project/project-tasks/task-delete/task-delete.component';
import { TaskMessagesComponent } from './components/project/project-tasks/task/task-messages/task-messages.component';
import { TaskMessageComponent } from './components/project/project-tasks/task/task-messages/task-message/task-message.component';
import { TaskMessageDeleteComponent } from './components/project/project-tasks/task/task-messages/task-message-delete/task-message-delete.component';
import { MessageService } from './services/message.service';

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
    InviteComponent,
    ProjectInfoComponent,
    ProjectTasksComponent,
    ProjectArchiveComponent,
    ProjectGanttComponent,
    ProjectParticipantsComponent,
    ProjectInfoShowComponent,
    ProjectInfoEditComponent,
    ProjectInfoDeleteComponent,
    ProjectInviteComponent,
    ProjectInviteRegenerateComponent,
    ProjectLeaveComponent,
    ProjectKickComponent,
    ProjectUserEditComponent,
    ProjectUserAddComponent,
    TaskReducedComponent,
    ScrollableDirective,
    ProjectTagDeleteComponent,
    TaskAddComponent,
    TaskComponent,
    TaskDeleteComponent,
    TaskMessagesComponent,
    TaskMessageComponent,
    TaskMessageDeleteComponent
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
    MatButtonToggleModule,
    MatSlideToggleModule,
    ClipboardModule,
    MatTableModule,
    MatTooltipModule,
    MatSelectModule,
    NgxMatSelectSearchModule,
    MatExpansionModule,
    MatChipsModule,
    NgxMatDatetimePickerModule,
    NgxMatTimepickerModule,
    NgxMatNativeDateModule,
    MatDatepickerModule
  ],
  providers: [
    GeneralinfoService,
    ProcessHTTPMsgService,
    {provide: 'baseURL', useValue: baseURL},
    {provide: 'appURL', useValue: appURL},
    {provide: 'emailConfirmationURL', useValue: emailConfirmationURL},
    {provide: 'projectName', useValue: projectName},
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
    UserInfoService,
    TokenGuardService,
    authInterceptor,
    UserService,
    TaskService,
    ProjectService,
    InviteService,
    ProjectRoleService,
    {
      provide: NgxMatDateAdapter,
      useClass: CustomNgxDatetimeAdapter,
      deps: [MAT_DATE_LOCALE, NGX_MAT_MOMENT_DATE_ADAPTER_OPTIONS]
    },
    { provide: NGX_MAT_DATE_FORMATS, useValue: CUSTOM_DATE_FORMATS },
    MessageService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
