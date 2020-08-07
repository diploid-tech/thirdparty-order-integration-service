CREATE TABLE "order"."order" (
    externalId varchar(64) PRIMARY KEY,
    hash varchar(128) NOT NULL,
    created timestamp with time zone NOT NULL
);
