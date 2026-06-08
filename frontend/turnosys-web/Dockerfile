FROM node:22-alpine AS build
WORKDIR /app
# Copiamos todo de una (incluye package-lock.json). Evita el bug de cache de
# BuildKit en Railway con COPY selectivos (package*.json no traía el lock).
COPY . .
RUN npm ci
RUN npm run build

FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
# Tomamos nginx.conf desde el stage build (ya copiado por "COPY . ."),
# evitando un error de checksum de BuildKit al copiarlo desde el contexto.
# Como template: la imagen oficial hace envsubst de ${PORT} al arrancar.
COPY --from=build /app/nginx.conf /etc/nginx/templates/default.conf.template
