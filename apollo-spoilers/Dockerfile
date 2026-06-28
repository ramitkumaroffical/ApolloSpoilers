# ---------- Frontend Dockerfile (multi-stage) ----------
# Stage 1: build Angular app
FROM node:20-alpine AS build

WORKDIR /app

COPY package*.json ./

# Use npm install instead of npm ci
RUN npm install

COPY . .

RUN npm run build -- --configuration production


# Stage 2: serve via nginx
FROM nginx:alpine AS runtime

# SPA-friendly nginx config
RUN printf 'server {\n\
  listen 80;\n\
  server_name _;\n\
  root /usr/share/nginx/html;\n\
  index index.html;\n\
  location / { try_files $uri $uri/ /index.html; }\n\
}\n' > /etc/nginx/conf.d/default.conf


COPY --from=build /app/dist/apollo-spoilers/browser /usr/share/nginx/html


EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]