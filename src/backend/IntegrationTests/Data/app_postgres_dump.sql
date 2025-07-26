--
-- PostgreSQL database dump
--

-- Dumped from database version 17.5 (Debian 17.5-1.pgdg120+1)
-- Dumped by pg_dump version 17.5 (Debian 17.5-1.pgdg120+1)

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
-- Name: Users; Type: TABLE; Schema: public; Owner: records
--

CREATE TABLE public."Users" (
    "Id" uuid NOT NULL,
    "Username" text NOT NULL,
    "Email" text NOT NULL,
    "PasswordHash" text NOT NULL,
    "PasswordSalt" text NOT NULL,
    "DateCreated" timestamp with time zone NOT NULL,
    "DateUpdated" timestamp with time zone NOT NULL
);


ALTER TABLE public."Users" OWNER TO records;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: records
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO records;

--
-- Data for Name: Users; Type: TABLE DATA; Schema: public; Owner: records
--

INSERT INTO public."Users" VALUES ('f7fdef01-1e73-4a83-a770-4a5148a919f3', 'alberteinstein', 'albert.einstein@example.invalid', 'EE0E3D3529133F9096ED2CAEC72BD07849D43F035DC3A2E9A2F30063E96DA482', '0C25358BD4BCDCCDDEB96E0EB918BF0A', '2025-02-19 17:21:32.782144+13', '2025-02-19 17:21:32.782144+13');
INSERT INTO public."Users" VALUES ('b378ee12-e261-47ff-8a8d-b202787bc631', 'stephenhawking', 'stephen.hawking@example.invalid', 'D9301F85F4F2F5400A8511499FA7E8F8AE82B38009138D64B944A8C22D8426A2', '4EE5B8A01B3E975AA35303F8F3529047', '2025-02-19 17:32:36.01753+13', '2025-02-19 17:32:36.01753+13');
INSERT INTO public."Users" VALUES ('362c8551-0fff-47fb-9ed3-9fb39828308c', 'mariecurie', 'marie.curie@example.invalid', 'C2F52C4390B3FA9DC11A3435A5025543D4830331CECB7F28516DAB733C4445F3', 'FC41FDB949D35BABD2B9DAD64BBB6AE4', '2025-02-19 17:32:29.592922+13', '2025-02-19 17:32:29.592922+13');


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: records
--

INSERT INTO public."__EFMigrationsHistory" VALUES ('20250724214309_InitialMigration', '9.0.7');


--
-- Name: Users PK_Users; Type: CONSTRAINT; Schema: public; Owner: records
--

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "PK_Users" PRIMARY KEY ("Id");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: records
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: IX_User_Email_UNIQUE; Type: INDEX; Schema: public; Owner: records
--

CREATE UNIQUE INDEX "IX_User_Email_UNIQUE" ON public."Users" USING btree ("Email");


--
-- Name: IX_User_Username_UNIQUE; Type: INDEX; Schema: public; Owner: records
--

CREATE UNIQUE INDEX "IX_User_Username_UNIQUE" ON public."Users" USING btree ("Username");


--
-- PostgreSQL database dump complete
--

