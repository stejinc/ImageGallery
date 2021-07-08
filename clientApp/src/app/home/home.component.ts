import { Component, OnInit } from '@angular/core';
import { AuthServiceService } from '../auth-service.service';
import { IImageContent } from '../IImageContent';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  allImages: IImageContent[];
  pageLoading = false;
  iconVisible = true;
  constructor(private authService: AuthServiceService) { }

  ngOnInit(): void {
    this.pageLoading = true;
    this.authService.getAllImages().subscribe(response => {
      console.log(response.message);
      if(response != null && response.status)
      {
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
  mouseEnter(){
    this.iconVisible = true;
  }

  mouseLeave(){
    this.iconVisible = false;
  }

}
