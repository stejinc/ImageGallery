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

    updateProfilePic(formData: FormData) : Observable<any> {
      return this.http.post(`${baseUrl}accounts/api/UpdateProfilePic`, formData);
    }

    getUserInfo() : Observable<any>{
      return this.http.get<any>(`${baseUrl}accounts/api/getUserDetails`);
    }
   
    login(data:any):Observable<any>{
      return this.http.post(`${baseUrl}accounts/api/login`, data);
    }

    register(data:any):Observable<any>{
      return this.http.post(`${baseUrl}accounts/api/register`, data);
    }

    getUserImages(pageSize:number, pageOffset:number):Observable<any>{
      return this.http.get<any>(`${baseUrl}home/api/image/${pageSize}/${pageOffset}`);
    }
    
    getSharedImages(pageSize:number, pageOffset:number, isLoggedIn: boolean):Observable<any>{
      return this.http.get<any>(`${baseUrl}home/api/sharedimages/${pageSize}/${pageOffset}/${isLoggedIn}`);
    }

    deleteImage(imageId : number):Observable<any>{
      return this.http.delete(`${baseUrl}home/api/image/${imageId}`);
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
