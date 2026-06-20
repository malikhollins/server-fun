FROM savonet/liquidsoap:v2.4.x-latest

USER root
RUN apt-get update && apt-get install -y awscli && rm -rf /var/lib/apt/lists/*

USER liquidsoap
COPY script.liq /etc/liquidsoap/script.liq

CMD ["/etc/liquidsoap/script.liq"]
