# ImageGallery
A simple web application to store and retrieve images.


Design
Frontend - Angular
Backend - Asp.net core web api
Database - MS Sql


Implemented with docker support.
Steps to run with docker
1.Install docker
2.Run docker-compose up

Current issues: Hardcoded backend url inside angular have to modify it to use localhost for local setup and the ip for web deployment

Features completed till now:
1.Login view
2.Register user
3.Signout
4.JWT authentication and authorization
5.View added images(Home page)
6.Profile setting view
7.Update profile picture
8.Update profile password ( Password is stored as SHA256 hash string in db )

InProgress:
1.Improve logs for both backend and front end.


Future enchancements:
1.Add pagination for images
2.Share image (Public, Private)
3.Share image view
4.Tag image
5.Integrate ML to analyse an image and generate tags
6.Chat interface for registered users
