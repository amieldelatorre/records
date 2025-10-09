FROM --platform=$BUILDPLATFORM nginx:1.29.2

COPY ./entrypoint.sh /usr/local/bin
COPY ./html/ /usr/share/nginx/html/
COPY ./nginx.conf /etc/nginx/conf.d/default.conf

# custom entrypoint because there are variables that should be overwritten
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]
# cmd is the same as original
CMD ["nginx", "-g", "daemon off;"]