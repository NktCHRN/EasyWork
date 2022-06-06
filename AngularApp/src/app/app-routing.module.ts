import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmailConfirmationComponent } from './email-confirmation/email-confirmation.component';
import { HomeComponent } from './home/home.component';
import { NotfoundComponent } from './notfound/notfound.component';
import { RegistrationComponent } from './registration/registration.component';

const routes: Routes = [
  { path: 'home',  component: HomeComponent, data: { title: 'Easy project management' } },
  { path: 'registration',  component: RegistrationComponent, data: { title: 'Registration' } },
  { path: 'emailconfirmation', component: EmailConfirmationComponent, data: { title: 'Email confirmation' } },
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: '**', pathMatch: 'full', 
  component: NotfoundComponent, data: { title: 'Not found' } },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
