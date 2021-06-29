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
        if(result.token){
          this.formGroup.enable();
          this.pageLoading = false;
          console.log(result);
          this.errorMsg = 'null';
          localStorage.setItem('token', result.token);
          this.routerService.navigate(['home']);
        }
      }, error => {
        this.formGroup.enable();
        this.pageLoading = false;
        console.log("error:" + error);
        this.errorMsg = "Login failed";
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
