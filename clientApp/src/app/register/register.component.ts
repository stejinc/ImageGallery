import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Router, RoutesRecognized } from '@angular/router';
import { AuthServiceService } from '../auth-service.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  registerFormGroup: FormGroup;
  errorMsg: string = "null";
  constructor(private formBuilder:FormBuilder,private authService:AuthServiceService, private routerService:Router) { }

  ngOnInit(): void {
    this.initForm();
  }

  registerProcess(){
    if(this.registerFormGroup.valid){
      
      const formBody = this.registerFormGroup.value;
      const formData = new FormData();

      formData.append("userName",formBody.userName);
      formData.append("password",formBody.password);
      formData.append("dateOfBirth",formBody.dateOfBirth.toLocaleDateString());
      formData.append("firstName",formBody.firstName);
      formData.append("lastName",formBody.lastName);
      formData.append("gender",formBody.gender);
      formData.append("profilePic",formBody.profilePic);
      this.authService.register(formData).subscribe(result =>{
        if(result.token){
          console.log(result);
          this.errorMsg = 'null';
          localStorage.setItem('token', result.token);
          this.routerService.navigate(['home']);
        }
      },
      error =>{
        this.errorMsg = "Registration Failed";
      });
    }
  }

  onFileSelect(event : Event){
    const target = event.target as HTMLInputElement;
    const files = target.files as FileList;
    if(files.length > 0)
      this.registerFormGroup.get('profilePic')?.setValue(files[0])
    else{
      this.errorMsg = "Invalid file selected"
    }
  }

  initForm(){
    this.registerFormGroup = new FormGroup({
      userName: new FormControl('',[Validators.required]),
      password: new FormControl('',[Validators.required]),
      confirmpassword: new FormControl('',[Validators.required]),
      firstName: new FormControl('',[Validators.required]),
      lastName: new FormControl(null),
      gender: new FormControl('',[Validators.required]),
      dateOfBirth: new FormControl('', [Validators.required]),
      profilePic: new FormControl(null)
    });
  }
}
