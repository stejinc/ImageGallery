import { HttpClient,HttpClientModule } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { baseUrl } from 'src/environments/environment';
import { IImageContent } from './IImageContent';
import { UserDetails } from './UserDetailsModel';

@Injectable({
  providedIn: 'root'
})
export class AuthServiceService {
  
  updatePassword(formGroupData: FormGroup) : Observable<any> {
    return this.http.post(`${baseUrl}accounts/api/UpdatePassword`, formGroupData);
  }

  constructor(private http:HttpClient) {
    
  }

    updateProfilePic(formData: FormData) {
      return this.http.post(`${baseUrl}accounts/api/UpdateProfilePic`, formData);
    }

    getUserInfo() : Observable<UserDetails>{
      return this.http.get<UserDetails>(`${baseUrl}accounts/api/getUserDetails`);
    }
   
    login(data:any):Observable<any>{
      return this.http.post(`${baseUrl}accounts/api/login`, data);
    }

    register(data:any):Observable<any>{
      return this.http.post(`${baseUrl}accounts/api/register`, data);
    }

    getAllImages():Observable<IImageContent[]>{
      return this.http.get<IImageContent[]>(`${baseUrl}home/api/image`);
    }

    addImage(data:FormData):Observable<any>{
      return this.http.post(`${baseUrl}home/api/upload`,data);
    }

    getToken(){
      return localStorage.getItem('token');
    }

    isLoggedIn(){
      return !!localStorage.getItem('token');
    }

    Logout(){
      localStorage.removeItem('token');
    }

}
