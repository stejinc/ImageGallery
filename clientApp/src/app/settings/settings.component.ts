import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { DomSanitizer } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { AuthServiceService } from '../auth-service.service';
import { UserDetails } from '../UserDetailsModel';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class SettingsComponent implements OnInit {
  updatePic : boolean = false;
  updatePassword : boolean = false;
  UsersInfo : UserDetails;
  changePassword : FormGroup;
  errorMsg : string = 'null';
  constructor(private authService : AuthServiceService , private sanitizer : DomSanitizer,
    private router : Router) {   }
 
  UpdateProfilePassword(){
    this.updatePic = false;
    this.updatePassword = true;
  }
  
  UpdateProfilePic(){
    this.updatePic = true;
    this.updatePassword = false;
  }
  
  loadDetails(){
    this.authService.getUserInfo().subscribe(result =>{
      if(result != null){
        this.UsersInfo = result;
        console.log(this.UsersInfo);
      }
    })
  }

  ngOnInit(): void {
    this.initFormGroup();
    this.loadDetails();
  }

  initFormGroup(){
    this.changePassword = new FormGroup({
      oldpassword: new FormControl('',[Validators.required]),
      newpassword: new FormControl('',[Validators.required])
    });
  }

  updateNewPassword(){
    
    if(this.changePassword.valid){
      this.authService.updatePassword(this.changePassword.value).subscribe(result => {
        if(result.token){
          console.log("Password updated succesfully");
          localStorage.removeItem('token');
          localStorage.setItem('token', result.token);
          this.updatePassword = false;
          this.errorMsg = "Password Updated";
          console.log(this.errorMsg);
        }
        else if(!result.token)
        {
          this.errorMsg = "Failed";
        }
      },
      error => {this.errorMsg = "Failed";})
    }
  }

  onFileSelect(event : Event){
    const target = event.target as HTMLInputElement;
    const files = target.files as FileList;
    if(files.length > 0)
    {  //this.imageFormGroup.get('image')?.setValue(files[0]);
      const formData = new FormData();
      formData.set('image',files[0]);
      this.authService.updateProfilePic(formData).subscribe(() => {
        this.updatePic = false;
        this.loadDetails();
      },
      () => {console.log("Error updating profile pic")})
    }
    else
    {
      //this.errorMsg = "Invalid file selected";
      console.log("Error uploading profile pic");
    }
  }

}
