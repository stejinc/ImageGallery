import { Component, OnInit } from '@angular/core';
import { AuthServiceService } from '../auth-service.service';
import { IImageContent } from '../IImageContent';

@Component({
  selector: 'app-sharedimages',
  templateUrl: './sharedimages.component.html',
  styleUrls: ['./sharedimages.component.css']
})
export class SharedimagesComponent implements OnInit {

  allImages: IImageContent[] = [];
  pageLoading: boolean = false;
  iconVisible: boolean = true;
  next : boolean = false;
  pageOffset = 0;
  pageSize = 8;
  PrevButtonStatus = true;
  NextButtonStatus = true;
  isLoggedIn = false;
  ZoomImage = false;
  ImageSrcZoom = 'null';

  constructor(private authService: AuthServiceService) { }

  IncrOffset(){
    this.LoadImages(this.pageOffset+1);
  }

  DcrOffset(){
    this.LoadImages(this.pageOffset-1);
  }

  LoadImages(updatedOffset: number){
    this.pageLoading = true;
    this.authService.getSharedImages(this.pageSize,updatedOffset,this.isLoggedIn).subscribe(response => {
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
    this.isLoggedIn = this.authService.isLoggedIn();
    this.LoadImages(this.pageOffset);
  }

  zoomImage(event : any){
    this.ZoomImage = true;
    this.ImageSrcZoom = event["src"];
  }

  HideZoom(){
    this.ZoomImage = false;
    this.ImageSrcZoom = 'null';
  }

}
