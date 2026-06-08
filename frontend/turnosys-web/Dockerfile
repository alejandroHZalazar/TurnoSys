# NOTA: el contexto de build es la RAÍZ del repo (Railway sin Root Directory),
# por eso los COPY referencian la subcarpeta frontend/turnosys-web/.
FROM node:22-alpine AS build
WORKDIR /app
COPY frontend/turnosys-web/ ./
RUN npm install
RUN npm run build

FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
# nginx.conf como template: la imagen oficial hace envsubst de ${PORT} al arrancar.
COPY --from=build /app/nginx.conf /etc/nginx/templates/default.conf.template
