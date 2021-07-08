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
  updatePic: boolean;
  updatePassword: boolean;
  UsersInfo: UserDetails;
  changePassword: FormGroup;
  errorMsg: string = 'null';
  constructor(private authService: AuthServiceService, private sanitizer: DomSanitizer,
    private router: Router) { }

  UpdateProfilePassword() {
    this.updatePic = false;
    this.updatePassword = true;
  }

  UpdateProfilePic() {
    this.updatePic = true;
    this.updatePassword = false;
  }

  loadDetails() {
    this.authService.getUserInfo().subscribe(response => {
      if (response != null && response.status) {
        console.log(response);
        this.UsersInfo = response.data;
        this.UsersInfo.profilePic = this.UsersInfo.profilePic == null ? "../../assets/Images/userimage.jpg" : "data:image/jpg;base64," + this.UsersInfo.profilePic;
        console.log(this.UsersInfo);
      }
      else {
        this.errorMsg = response.message ?? "Failed to retrieve data";
      }
    })
  }

  ngOnInit(): void {
    // this.UsersInfo.profilePic = "../../assets/Images/userimage.jpg";
    this.UsersInfo = new UserDetails();
    this.UsersInfo.profilePic = "../../assets/Images/userimage.jpg";
    this.initFormGroup();
    this.loadDetails();
    this.updatePic = false;
    this.updatePassword = false;
  }

  initFormGroup() {
    this.changePassword = new FormGroup({
      oldpassword: new FormControl('', [Validators.required]),
      newpassword: new FormControl('', [Validators.required])
    });
  }

  updateNewPassword() {
    this.errorMsg = 'null';
    if (this.changePassword.valid) {
      this.authService.updatePassword(this.changePassword.value).subscribe(result => {
        if (result != null && result.status) {
          console.log("Password updated succesfully");
          this.errorMsg = "Password Updated";
          if(result.token){
            localStorage.removeItem('token');
            localStorage.setItem('token', result.token);
            this.updatePassword = false;
            console.log(this.errorMsg);
          }else
            this.errorMsg = result.message ?? "Operation failed";
        }
        else
          this.errorMsg = result.message ?? "Operation failed";
      },
        error => { 
          this.errorMsg = error.error.message ?? "Operation failed";
         }
      )
    }
  }

  onFileSelect(event: Event) {
    const target = event.target as HTMLInputElement;
    const files = target.files as FileList;
    if (files.length > 0) {  //this.imageFormGroup.get('image')?.setValue(files[0]);
      const formData = new FormData();
      formData.set('image', files[0]);
      this.authService.updateProfilePic(formData).subscribe((response) => {
        if (response != null && response.status) {
          this.errorMsg = "Profile image updated";
          this.updatePic = false;
          this.loadDetails();
        }
        else {
          this.errorMsg = "Operation failed";
        }
      },
        () => { console.log("Error updating profile pic");
        this.errorMsg = "Error updating profile pic";
      })
    }
    else {
      //this.errorMsg = "Invalid file selected";
      console.log("Error updating profile pic");
    }
  }

}
