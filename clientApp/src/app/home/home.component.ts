import { Component, OnInit } from '@angular/core';
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

  constructor(private authService: AuthServiceService) { }

  IncrOffset(){
    this.LoadImages(this.pageOffset+1);
  }

  DcrOffset(){
    this.LoadImages(this.pageOffset-1);
  }

  LoadImages(updatedOffset: number){
    this.pageLoading = true;
    this.authService.getAllImages(this.pageSize,updatedOffset).subscribe(response => {
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
  mouseEnter(){
    this.iconVisible = true;
  }

  mouseLeave(){
    this.iconVisible = false;
  }

}
