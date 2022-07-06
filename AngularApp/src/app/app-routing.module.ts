import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AnonymousGuard } from './anonymous.guard';
import { AuthGuard } from './auth.guard';
import { CabinetComponent } from './cabinet/cabinet.component';
import { EmailConfirmationComponent } from './email-confirmation/email-confirmation.component';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { NotfoundComponent } from './notfound/notfound.component';
import { ProfileComponent } from './profile/profile.component';
import { RegistrationComponent } from './registration/registration.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';

const routes: Routes = [
  { path: 'home',  component: HomeComponent, data: { title: 'Easy project management' } },
  { path: 'login',  component: LoginComponent, data: { title: 'Login' }, canActivate: [AnonymousGuard] },
  { path: 'registration',  component: RegistrationComponent, data: { title: 'Registration' }, canActivate: [AnonymousGuard] },
  { path: 'emailconfirmation', component: EmailConfirmationComponent, data: { title: 'Email confirmation' } },
  { path: 'resetpassword', component: ResetPasswordComponent, data: { title: 'Reset password' } },
  { path: 'cabinet',  component: CabinetComponent, data: { title: 'Cabinet' }, canActivate: [AuthGuard] },
  { path: 'users/:id',  component: ProfileComponent, data: { title: 'Profile' }, canActivate: [AuthGuard] },
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: '**', pathMatch: 'full', 
  component: NotfoundComponent, data: { title: 'Not found' } },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
