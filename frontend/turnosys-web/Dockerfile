FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
# Como template: la imagen oficial hace envsubst de ${PORT} al arrancar
# y genera /etc/nginx/conf.d/default.conf antes de iniciar nginx.
COPY nginx.conf /etc/nginx/templates/default.conf.template
