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
      
      this.authService.addImage(formData).subscribe(response => {
        if(response != null && response.status){
          console.log(response);
          this.imageFormGroup.reset();
          this.pageLoading = false;
          this.errorMsg = response.message ?? "Upload successful";
        }
        else{
          this.pageLoading = false;
          this.errorMsg = response.message ?? "Upload failed";
        }

      },
      error=>{
        this.pageLoading = false;
        this.errorMsg = "Upload failed";
      });
    }
  }

  initForm(){
    this.imageFormGroup = new FormGroup({
      image : new FormControl('', [Validators.required]),
      description: new FormControl('')
    });
  }

  onFileSelect(event : Event){
    this.errorMsg = 'null';
    const target = event.target as HTMLInputElement;
    const files = target.files as FileList;
    if(files.length > 0)
      this.imageFormGroup.get('image')?.setValue(files[0]);
    else
      this.errorMsg = "Invalid file selected";
  }

}
