import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthServiceService } from '../auth-service.service';
import { IImageContent } from '../IImageContent';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  allImages: IImageContent[] = [];
  pageLoading: boolean = false;
  iconVisible: boolean = true;
  next : boolean = false;
  pageOffset = 0;
  pageSize = 8;
  PrevButtonStatus = true;
  NextButtonStatus = true;
  ZoomImage = false;
  ImageSrcZoom = 'null';
  SelectedImageId = -1;
  errorMsg = 'null';

  constructor(private authService: AuthServiceService, private routerService : Router) { }

  IncrOffset(){
    this.LoadImages(this.pageOffset+1);
  }

  DcrOffset(){
    this.LoadImages(this.pageOffset-1);
  }

  LoadImages(updatedOffset: number){
    this.pageLoading = true;
    this.authService.getUserImages(this.pageSize,updatedOffset).subscribe(response => {
      console.log(response.message);
      if(response != null && response.status)
      {
        this.pageOffset = updatedOffset;
        this.next = response.isNext;
        if(this.pageOffset == 0)
          this.PrevButtonStatus = true;
        else
          this.PrevButtonStatus = false;
        if(!this.next)
          this.NextButtonStatus = true;
        else
          this.NextButtonStatus = false;
        this.pageLoading = false;
        this.allImages = response.data;
      }
      else{
        this.allImages = [];
      }
    },
    error => {
      this.pageLoading = false;
    })
  }

  ngOnInit(): void {
    this.LoadImages(this.pageOffset);
  }

  zoomImage(event : any){
    this.ZoomImage = true;
    this.ImageSrcZoom = event["src"];
    this.SelectedImageId = event["id"];
  }

  DeleteImage(){
    if(this.SelectedImageId != -1)
    {
      this.authService.deleteImage(this.SelectedImageId).subscribe(result => {
        if(result.status){
          console.log("Deletion successful");
          this.LoadImages(this.pageOffset);
          this.ZoomImage = false;
          console.log(this.ZoomImage);
          this.SelectedImageId = -1;
          console.log(this.SelectedImageId);
        }
        else{
          this.errorMsg = "Failed to delete image";
        }
      },
      error =>{
          this.errorMsg = error.error.message;
      });
    }
  }

  HideZoom(){
    this.ZoomImage = false;
    this.ImageSrcZoom = 'null';
    this.SelectedImageId = -1;
  }

  mouseEnter(){
    this.iconVisible = true;
  }

  mouseLeave(){
    this.iconVisible = false;
  }

}
