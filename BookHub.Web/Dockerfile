# Stage 1: build the static bundle
FROM node:24-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# The API URL is baked into the bundle at build time (the browser calls the API directly).
ARG VITE_API_URL=http://localhost:5254
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

# Stage 2: serve it with nginx
FROM nginx:alpine AS final
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
