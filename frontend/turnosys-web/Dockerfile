FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
# Tomamos nginx.conf desde el stage build (ya copiado por "COPY . ."),
# evitando un error de checksum de BuildKit al copiarlo desde el contexto.
# Como template: la imagen oficial hace envsubst de ${PORT} al arrancar.
COPY --from=build /app/nginx.conf /etc/nginx/templates/default.conf.template
