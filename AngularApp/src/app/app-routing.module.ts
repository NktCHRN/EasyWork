import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AnonymousGuard } from './guards/anonymous.guard';
import { AuthGuard } from './guards/auth.guard';
import { CabinetComponent } from './components/cabinet/cabinet.component';
import { EmailConfirmationComponent } from './components/email-confirmation/email-confirmation.component';
import { HomeComponent } from './components/home/home.component';
import { LoginComponent } from './components/login/login.component';
import { NotfoundComponent } from './components/notfound/notfound.component';
import { ProfileComponent } from './components/profile/profile.component';
import { ProjectComponent } from './components/project/project.component';
import { ProjectsComponent } from './components/projects/projects.component';
import { RegistrationComponent } from './components/registration/registration.component';
import { ResetPasswordComponent } from './components/reset-password/reset-password.component';
import { TasksComponent } from './components/tasks/tasks.component';
import { InviteComponent } from './components/invite/invite.component';

const routes: Routes = [
  { path: 'home',  component: HomeComponent, data: { title: 'Easy project management' } },
  { path: 'login',  component: LoginComponent, data: { title: 'Login' }, canActivate: [AnonymousGuard] },
  { path: 'registration',  component: RegistrationComponent, data: { title: 'Registration' }, canActivate: [AnonymousGuard] },
  { path: 'emailconfirmation', component: EmailConfirmationComponent, data: { title: 'Email confirmation' } },
  { path: 'resetpassword', component: ResetPasswordComponent, data: { title: 'Reset password' } },
  { path: 'cabinet',  component: CabinetComponent, data: { title: 'Cabinet' }, canActivate: [AuthGuard] },
  { path: 'users/:id',  component: ProfileComponent, data: { title: 'Profile' } },
  { path: 'tasks',  component: TasksComponent, data: { title: 'My tasks' }, canActivate: [AuthGuard] },
  { path: 'projects',  component: ProjectsComponent, data: { title: 'My projects' }, canActivate: [AuthGuard] },
  { path: 'projects/:id',  component: ProjectComponent, data: { title: 'Project' }, canActivate: [AuthGuard] },
  { path: 'invite/:code',  component: InviteComponent, data: { title: 'Invite' }, canActivate: [AuthGuard] },
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: '**', pathMatch: 'full', 
  component: NotfoundComponent, data: { title: 'Not found' } },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
