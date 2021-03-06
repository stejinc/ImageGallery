import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ThemePalette } from '@angular/material/core';
import { ProgressSpinnerMode } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { AuthServiceService } from '../auth-service.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  formGroup: FormGroup;
  errorMsg: string = 'null';
  pageLoading = false;
  @Output() notify: EventEmitter<string> = new EventEmitter<string>();

  constructor(private authService : AuthServiceService, private routerService: Router) { }

  loginProcess(){
    if(this.formGroup.valid){
      this.pageLoading = true;
      this.formGroup.disable();
      this.authService.login(this.formGroup.value).subscribe(result=>{
        if(result != null && result.status){
          this.errorMsg = result.message ?? "Login Successful";
          this.formGroup.enable();
          this.pageLoading = false;
          console.log(result);
          localStorage.setItem('token', result.token);
          this.routerService.navigate(['sharedimages']);
        }
      }, error => {
        this.formGroup.enable();
        this.pageLoading = false;
        console.log("error:" + error.error);
        this.errorMsg = error.error.message ?? "Login failed";
      });
    }
  }

  ngOnInit(): void {
    this.initForm();
  }
  initForm(){
    this.formGroup = new FormGroup({
      username:new FormControl('',[Validators.required,Validators.email]),
      password:new FormControl('',[Validators.required])
    });
  }

}
