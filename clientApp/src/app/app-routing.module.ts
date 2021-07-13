import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AddImageComponent } from './add-image/add-image.component';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { SettingsComponent } from './settings/settings.component';
import { SharedimagesComponent } from './sharedimages/sharedimages.component';

const routes: Routes = [
  {path: '', component: SharedimagesComponent},
  {path: 'login', component: LoginComponent},
  {path: 'home', component: HomeComponent},
  {path: 'register', component: RegisterComponent},
  {path: 'settings', component: SettingsComponent},
  {path: 'sharedimages', component: SharedimagesComponent},
  {path: 'home/addImage', component: AddImageComponent}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
