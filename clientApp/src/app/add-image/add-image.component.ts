import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthServiceService } from '../auth-service.service';

@Component({
  selector: 'app-add-image',
  templateUrl: './add-image.component.html',
  styleUrls: ['./add-image.component.css']
})
export class AddImageComponent implements OnInit {
  imageFormGroup : FormGroup;
  errorMsg: string = "null";
  pageLoading = false;

  constructor(private authService: AuthServiceService, private routerService:Router) { }

  ngOnInit(): void {
    this.initForm();
  }

  uploadImage(){
    this.pageLoading = true;
    if(this.imageFormGroup.valid){
      const formBody= this.imageFormGroup.value;
      const formData = new FormData();

      formData.set('image', formBody.image);
      formData.set('description', formBody.description);
      
      this.authService.addImage(formData).subscribe(result => {
        console.log(result);
        this.pageLoading = false;
        this.routerService.navigate(['home']);
      },
      error=>{
        this.pageLoading = false;
        this.errorMsg = "Upload failed";
      })
    }
  }

  initForm(){
    this.imageFormGroup = new FormGroup({
      image : new FormControl('', [Validators.required]),
      description: new FormControl('')
    });
  }

  onFileSelect(event : Event){
    const target = event.target as HTMLInputElement;
    const files = target.files as FileList;
    if(files.length > 0)
      this.imageFormGroup.get('image')?.setValue(files[0]);
    else
      this.errorMsg = "Invalid file selected";
  }

}
