--
-- PostgreSQL database dump
--

-- Dumped from database version 17.0 (Debian 17.0-1.pgdg120+1)
-- Dumped by pg_dump version 17.0 (Debian 17.0-1.pgdg120+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: Users; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public."Users" (
    "Id" uuid NOT NULL,
    "FirstName" text NOT NULL,
    "LastName" text NOT NULL,
    "Email" text NOT NULL,
    "PasswordHash" text NOT NULL,
    "PasswordSalt" text NOT NULL,
    "DateCreated" timestamp with time zone NOT NULL,
    "DateUpdated" timestamp with time zone NOT NULL
);


ALTER TABLE public."Users" OWNER TO root;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO root;

--
-- Data for Name: Users; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public."Users" VALUES ('571fac2e-317c-417e-982d-be2943edb07e', 'Albert', 'Einstein', 'albert.einstein@records.invalid', 'F75667E28F8F15CF967E644E16E07D77DA3D799F9E7D985E83B43CD47F31D17F', '527165C140FADE51BE6D37532278A3B3', '2024-10-03 16:06:32.85099+13', '2024-10-03 16:06:32.85099+13');
INSERT INTO public."Users" VALUES ('719b57e8-0a85-403e-9742-43ace59fe88d', 'Marie', 'Curie', 'marie.curie@records.invalid', '7817A06DE6A943C84B9F96FBB9DB1067A9F6F629F24C9321F1563AC356AC6AE4', '457D74EACDE63EC3569EEE0C9B8CCB6E', '2024-10-03 16:06:59.456156+13', '2024-10-03 16:06:59.456156+13');
INSERT INTO public."Users" VALUES ('3e098063-d9a4-4b24-9088-7a548b92796a', 'Stephen', 'Hawking', 'stephen.hawking@records.invalid', '24C864545CEB4C43A8F23F53926EC38C8F23C0C860E038F1809D10779CE607A3', '8237C3A6E776378316D916E03F00C688', '2024-10-03 16:07:32.427381+13', '2024-10-03 16:07:32.427381+13');


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public."__EFMigrationsHistory" VALUES ('20241003030549_InitialMigration', '8.0.8');


--
-- Name: Users PK_Users; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "PK_Users" PRIMARY KEY ("Id");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: IX_USER_EMAIL_UNIQUE; Type: INDEX; Schema: public; Owner: root
--

CREATE UNIQUE INDEX "IX_USER_EMAIL_UNIQUE" ON public."Users" USING btree ("Email");


--
-- PostgreSQL database dump complete
--

