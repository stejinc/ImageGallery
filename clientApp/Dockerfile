# stage 1
FROM node:latest as node
WORKDIR /app
COPY . .
RUN npm install
RUN npm run build --prod

# stage 2
FROM nginx
EXPOSE 8060
COPY ./nginx/default.conf /etc/nginx/conf.d/default.conf
COPY --from=node /app/dist/PhotoAppUI /usr/share/nginx/html