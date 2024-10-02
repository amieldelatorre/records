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
    "Password" text NOT NULL,
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

INSERT INTO public."Users" VALUES ('61bbe4bd-6693-49c8-9968-390de9dd0307', 'Albert', 'Einstein', 'albert.einstein@records.invalid', 'password1', '2024-10-02 18:45:06.452346+13', '2024-10-02 18:45:06.452346+13');
INSERT INTO public."Users" VALUES ('7f6ee7d3-a8b4-4a28-8d76-bb7fb199e160', 'Marie', 'Curie', 'marie.curie@records.invalid', 'password123214', '2024-10-02 18:45:46.252698+13', '2024-10-02 18:45:46.252698+13');
INSERT INTO public."Users" VALUES ('815b5a1c-7e94-4bb3-b1f3-49b0621dfb59', 'Stephen', 'Hawking', 'stephen.hawking@example.invalid', 'password', '2024-10-02 18:46:25.161097+13', '2024-10-02 18:46:25.161097+13');


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public."__EFMigrationsHistory" VALUES ('20240927203759_InitialMigration', '8.0.8');
INSERT INTO public."__EFMigrationsHistory" VALUES ('20241001055642_BaseEntityTimestampWithTimeZones', '8.0.8');


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

