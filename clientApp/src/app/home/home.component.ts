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
    this.authService.getAllImages().subscribe(result => {
      this.pageLoading = false;
      if(result != null){
        this.allImages = result;
        console.log(this.allImages);
      }
      else
        this.allImages = [];
    },
    error => {
      this.pageLoading = false;
      console.log("Error occoured" + error);
    })
  }
  mouseEnter(){
    this.iconVisible = true;
  }

  mouseLeave(){
    this.iconVisible = false;
  }

}
