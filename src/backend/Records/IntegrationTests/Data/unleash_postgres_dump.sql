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

--
-- Name: assign_unleash_permission_to_role(text, text); Type: FUNCTION; Schema: public; Owner: root
--

CREATE FUNCTION public.assign_unleash_permission_to_role(permission_name text, role_name text) RETURNS void
    LANGUAGE plpgsql
    AS $$
declare
    var_role_id int;
    var_permission text;
BEGIN
    var_role_id := (SELECT r.id FROM roles r WHERE r.name = role_name);
    var_permission := (SELECT p.permission FROM permissions p WHERE p.permission = permission_name);

    IF NOT EXISTS (
        SELECT 1
        FROM role_permission AS rp
        WHERE rp.role_id = var_role_id AND rp.permission = var_permission
    ) THEN
        INSERT INTO role_permission(role_id, permission) VALUES (var_role_id, var_permission);
    END IF;
END
$$;


ALTER FUNCTION public.assign_unleash_permission_to_role(permission_name text, role_name text) OWNER TO root;

--
-- Name: assign_unleash_permission_to_role_for_all_environments(text, text); Type: FUNCTION; Schema: public; Owner: root
--

CREATE FUNCTION public.assign_unleash_permission_to_role_for_all_environments(permission_name text, role_name text) RETURNS void
    LANGUAGE plpgsql
    AS $$
declare
    var_role_id int;
    var_permission text;
BEGIN
    var_role_id := (SELECT id FROM roles r WHERE r.name = role_name);
    var_permission := (SELECT p.permission FROM permissions p WHERE p.permission = permission_name);

    INSERT INTO role_permission (role_id, permission, environment)
        SELECT var_role_id, var_permission, e.name
        FROM environments e
        WHERE NOT EXISTS (
            SELECT 1
            FROM role_permission rp
            WHERE rp.role_id = var_role_id
            AND rp.permission = var_permission
            AND rp.environment = e.name
        );
END;
$$;


ALTER FUNCTION public.assign_unleash_permission_to_role_for_all_environments(permission_name text, role_name text) OWNER TO root;

--
-- Name: date_floor_round(timestamp with time zone, interval); Type: FUNCTION; Schema: public; Owner: root
--

CREATE FUNCTION public.date_floor_round(base_date timestamp with time zone, round_interval interval) RETURNS timestamp with time zone
    LANGUAGE sql STABLE
    AS $_$
SELECT to_timestamp(
    (EXTRACT(epoch FROM $1)::integer / EXTRACT(epoch FROM $2)::integer)
    * EXTRACT(epoch FROM $2)::integer
)
$_$;


ALTER FUNCTION public.date_floor_round(base_date timestamp with time zone, round_interval interval) OWNER TO root;

--
-- Name: unleash_update_stat_environment_changes_counter(); Type: FUNCTION; Schema: public; Owner: root
--

CREATE FUNCTION public.unleash_update_stat_environment_changes_counter() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
        BEGIN
            IF NEW.environment IS NOT NULL THEN
                INSERT INTO stat_environment_updates(day, environment, updates) SELECT DATE_TRUNC('Day', NEW.created_at), NEW.environment, 1 ON CONFLICT (day, environment) DO UPDATE SET updates = stat_environment_updates.updates + 1;
            END IF;

            return null;
        END;
    $$;


ALTER FUNCTION public.unleash_update_stat_environment_changes_counter() OWNER TO root;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: action_set_events; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.action_set_events (
    id integer NOT NULL,
    action_set_id integer NOT NULL,
    signal_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    state text NOT NULL,
    signal jsonb NOT NULL,
    action_set jsonb NOT NULL
);


ALTER TABLE public.action_set_events OWNER TO root;

--
-- Name: action_set_events_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.action_set_events_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.action_set_events_id_seq OWNER TO root;

--
-- Name: action_set_events_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.action_set_events_id_seq OWNED BY public.action_set_events.id;


--
-- Name: action_sets; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.action_sets (
    id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    created_by_user_id integer,
    name character varying(255),
    project character varying(255) NOT NULL,
    actor_id integer,
    source character varying(255),
    source_id integer,
    payload jsonb DEFAULT '{}'::jsonb NOT NULL,
    enabled boolean DEFAULT true,
    description text
);


ALTER TABLE public.action_sets OWNER TO root;

--
-- Name: action_sets_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.action_sets_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.action_sets_id_seq OWNER TO root;

--
-- Name: action_sets_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.action_sets_id_seq OWNED BY public.action_sets.id;


--
-- Name: actions; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.actions (
    id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    created_by_user_id integer,
    action_set_id integer,
    sort_order integer,
    action character varying(255) NOT NULL,
    execution_params jsonb DEFAULT '{}'::jsonb NOT NULL
);


ALTER TABLE public.actions OWNER TO root;

--
-- Name: actions_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.actions_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.actions_id_seq OWNER TO root;

--
-- Name: actions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.actions_id_seq OWNED BY public.actions.id;


--
-- Name: addons; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.addons (
    id integer NOT NULL,
    provider text NOT NULL,
    description text,
    enabled boolean DEFAULT true,
    parameters json,
    events json,
    created_at timestamp with time zone DEFAULT now(),
    projects jsonb DEFAULT '[]'::jsonb,
    environments jsonb DEFAULT '[]'::jsonb
);


ALTER TABLE public.addons OWNER TO root;

--
-- Name: addons_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.addons_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.addons_id_seq OWNER TO root;

--
-- Name: addons_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.addons_id_seq OWNED BY public.addons.id;


--
-- Name: api_token_project; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.api_token_project (
    secret text NOT NULL,
    project text NOT NULL
);


ALTER TABLE public.api_token_project OWNER TO root;

--
-- Name: api_tokens; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.api_tokens (
    secret text NOT NULL,
    username text NOT NULL,
    type text NOT NULL,
    created_at timestamp with time zone DEFAULT now(),
    expires_at timestamp with time zone,
    seen_at timestamp with time zone,
    environment character varying,
    alias text,
    token_name text,
    created_by_user_id integer
);


ALTER TABLE public.api_tokens OWNER TO root;

--
-- Name: banners; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.banners (
    id integer NOT NULL,
    enabled boolean DEFAULT true NOT NULL,
    message text NOT NULL,
    variant text,
    sticky boolean DEFAULT false,
    icon text,
    link text,
    link_text text,
    dialog_title text,
    dialog text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.banners OWNER TO root;

--
-- Name: change_request_approvals; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.change_request_approvals (
    id integer NOT NULL,
    change_request_id integer NOT NULL,
    created_by integer NOT NULL,
    created_at timestamp with time zone DEFAULT now()
);


ALTER TABLE public.change_request_approvals OWNER TO root;

--
-- Name: change_request_approvals_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.change_request_approvals_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.change_request_approvals_id_seq OWNER TO root;

--
-- Name: change_request_approvals_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.change_request_approvals_id_seq OWNED BY public.change_request_approvals.id;


--
-- Name: change_request_comments; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.change_request_comments (
    id integer NOT NULL,
    change_request integer NOT NULL,
    text text NOT NULL,
    created_at timestamp with time zone DEFAULT now(),
    created_by integer NOT NULL
);


ALTER TABLE public.change_request_comments OWNER TO root;

--
-- Name: change_request_comments_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.change_request_comments_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.change_request_comments_id_seq OWNER TO root;

--
-- Name: change_request_comments_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.change_request_comments_id_seq OWNED BY public.change_request_comments.id;


--
-- Name: change_request_events; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.change_request_events (
    id integer NOT NULL,
    feature character varying(255),
    action character varying(255) NOT NULL,
    payload jsonb DEFAULT '[]'::jsonb NOT NULL,
    created_by integer NOT NULL,
    created_at timestamp with time zone DEFAULT now(),
    change_request_id integer NOT NULL
);


ALTER TABLE public.change_request_events OWNER TO root;

--
-- Name: change_request_events_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.change_request_events_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.change_request_events_id_seq OWNER TO root;

--
-- Name: change_request_events_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.change_request_events_id_seq OWNED BY public.change_request_events.id;


--
-- Name: change_request_rejections; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.change_request_rejections (
    id integer NOT NULL,
    change_request_id integer NOT NULL,
    created_by integer NOT NULL,
    created_at timestamp with time zone DEFAULT now()
);


ALTER TABLE public.change_request_rejections OWNER TO root;

--
-- Name: change_request_rejections_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.change_request_rejections_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.change_request_rejections_id_seq OWNER TO root;

--
-- Name: change_request_rejections_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.change_request_rejections_id_seq OWNED BY public.change_request_rejections.id;


--
-- Name: change_request_schedule; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.change_request_schedule (
    change_request integer NOT NULL,
    scheduled_at timestamp without time zone NOT NULL,
    created_by integer,
    status text,
    failure_reason text,
    reason text
);


ALTER TABLE public.change_request_schedule OWNER TO root;

--
-- Name: change_request_settings; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.change_request_settings (
    project character varying(255) NOT NULL,
    environment character varying(100) NOT NULL,
    required_approvals integer DEFAULT 1
);


ALTER TABLE public.change_request_settings OWNER TO root;

--
-- Name: change_requests; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.change_requests (
    id integer NOT NULL,
    environment character varying(100),
    state character varying(255) NOT NULL,
    project character varying(255),
    created_by integer NOT NULL,
    created_at timestamp with time zone DEFAULT now(),
    min_approvals integer DEFAULT 1,
    title text
);


ALTER TABLE public.change_requests OWNER TO root;

--
-- Name: change_requests_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.change_requests_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.change_requests_id_seq OWNER TO root;

--
-- Name: change_requests_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.change_requests_id_seq OWNED BY public.change_requests.id;


--
-- Name: client_applications; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.client_applications (
    app_name character varying(255) NOT NULL,
    created_at timestamp with time zone DEFAULT now(),
    updated_at timestamp with time zone DEFAULT now(),
    seen_at timestamp with time zone,
    strategies json,
    description character varying(255),
    icon character varying(255),
    url character varying(255),
    color character varying(255),
    announced boolean DEFAULT false,
    created_by text
);


ALTER TABLE public.client_applications OWNER TO root;

--
-- Name: client_applications_usage; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.client_applications_usage (
    app_name character varying(255) NOT NULL,
    project character varying(255) NOT NULL,
    environment character varying(100) NOT NULL
);


ALTER TABLE public.client_applications_usage OWNER TO root;

--
-- Name: client_instances; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.client_instances (
    app_name character varying(255) NOT NULL,
    instance_id character varying(255) NOT NULL,
    client_ip character varying(255),
    last_seen timestamp with time zone DEFAULT now(),
    created_at timestamp with time zone DEFAULT now(),
    sdk_version character varying(255),
    environment character varying(255) DEFAULT 'default'::character varying NOT NULL
);


ALTER TABLE public.client_instances OWNER TO root;

--
-- Name: client_metrics_env; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.client_metrics_env (
    feature_name character varying(255) NOT NULL,
    app_name character varying(255) NOT NULL,
    environment character varying(100) NOT NULL,
    "timestamp" timestamp with time zone NOT NULL,
    yes bigint DEFAULT 0,
    no bigint DEFAULT 0
);


ALTER TABLE public.client_metrics_env OWNER TO root;

--
-- Name: client_metrics_env_daily; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.client_metrics_env_daily (
    feature_name character varying(255) NOT NULL,
    app_name character varying(255) NOT NULL,
    environment character varying(100) NOT NULL,
    date date NOT NULL,
    yes bigint DEFAULT 0,
    no bigint DEFAULT 0
);


ALTER TABLE public.client_metrics_env_daily OWNER TO root;

--
-- Name: client_metrics_env_variants; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.client_metrics_env_variants (
    feature_name character varying(255) NOT NULL,
    app_name character varying(255) NOT NULL,
    environment character varying(100) NOT NULL,
    "timestamp" timestamp with time zone NOT NULL,
    variant text NOT NULL,
    count integer DEFAULT 0
);


ALTER TABLE public.client_metrics_env_variants OWNER TO root;

--
-- Name: client_metrics_env_variants_daily; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.client_metrics_env_variants_daily (
    feature_name character varying(255) NOT NULL,
    app_name character varying(255) NOT NULL,
    environment character varying(100) NOT NULL,
    date date NOT NULL,
    variant text NOT NULL,
    count bigint DEFAULT 0
);


ALTER TABLE public.client_metrics_env_variants_daily OWNER TO root;

--
-- Name: context_fields; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.context_fields (
    name character varying(255) NOT NULL,
    description text,
    sort_order integer DEFAULT 10,
    legal_values json,
    created_at timestamp without time zone DEFAULT now(),
    updated_at timestamp without time zone DEFAULT now(),
    stickiness boolean DEFAULT false
);


ALTER TABLE public.context_fields OWNER TO root;

--
-- Name: dependent_features; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.dependent_features (
    parent character varying(255) NOT NULL,
    child character varying(255) NOT NULL,
    enabled boolean DEFAULT true NOT NULL,
    variants jsonb DEFAULT '[]'::jsonb NOT NULL
);


ALTER TABLE public.dependent_features OWNER TO root;

--
-- Name: environment_type_trends; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.environment_type_trends (
    id character varying(255) NOT NULL,
    environment_type character varying(255) NOT NULL,
    total_updates integer NOT NULL,
    created_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.environment_type_trends OWNER TO root;

--
-- Name: environments; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.environments (
    name character varying(100) NOT NULL,
    created_at timestamp with time zone DEFAULT now(),
    sort_order integer DEFAULT 9999,
    type text NOT NULL,
    enabled boolean DEFAULT true,
    protected boolean DEFAULT false
);


ALTER TABLE public.environments OWNER TO root;

--
-- Name: events; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.events (
    id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now(),
    type character varying(255) NOT NULL,
    created_by character varying(255) NOT NULL,
    data json,
    tags json DEFAULT '[]'::json,
    project text,
    environment text,
    feature_name text,
    pre_data jsonb,
    announced boolean DEFAULT false NOT NULL,
    created_by_user_id integer,
    ip text
);


ALTER TABLE public.events OWNER TO root;

--
-- Name: events_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.events_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.events_id_seq OWNER TO root;

--
-- Name: events_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.events_id_seq OWNED BY public.events.id;


--
-- Name: favorite_features; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.favorite_features (
    feature character varying(255) NOT NULL,
    user_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.favorite_features OWNER TO root;

--
-- Name: favorite_projects; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.favorite_projects (
    project character varying(255) NOT NULL,
    user_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.favorite_projects OWNER TO root;

--
-- Name: feature_environments; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.feature_environments (
    environment character varying(100) DEFAULT 'default'::character varying NOT NULL,
    feature_name character varying(255) NOT NULL,
    enabled boolean NOT NULL,
    variants jsonb DEFAULT '[]'::jsonb NOT NULL,
    last_seen_at timestamp with time zone
);


ALTER TABLE public.feature_environments OWNER TO root;

--
-- Name: feature_lifecycles; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.feature_lifecycles (
    feature character varying(255) NOT NULL,
    stage character varying(255) NOT NULL,
    created_at timestamp with time zone DEFAULT now(),
    status text,
    status_value text
);


ALTER TABLE public.feature_lifecycles OWNER TO root;

--
-- Name: feature_strategies; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.feature_strategies (
    id text NOT NULL,
    feature_name character varying(255) NOT NULL,
    project_name character varying(255) NOT NULL,
    environment character varying(100) DEFAULT 'default'::character varying NOT NULL,
    strategy_name character varying(255) NOT NULL,
    parameters jsonb DEFAULT '{}'::jsonb NOT NULL,
    constraints jsonb,
    sort_order integer DEFAULT 9999 NOT NULL,
    created_at timestamp with time zone DEFAULT now(),
    title text,
    disabled boolean DEFAULT false,
    variants jsonb DEFAULT '[]'::jsonb NOT NULL,
    created_by_user_id integer
);


ALTER TABLE public.feature_strategies OWNER TO root;

--
-- Name: feature_strategy_segment; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.feature_strategy_segment (
    feature_strategy_id text NOT NULL,
    segment_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.feature_strategy_segment OWNER TO root;

--
-- Name: feature_tag; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.feature_tag (
    feature_name character varying(255) NOT NULL,
    tag_type text NOT NULL,
    tag_value text NOT NULL,
    created_at timestamp with time zone DEFAULT now(),
    created_by_user_id integer
);


ALTER TABLE public.feature_tag OWNER TO root;

--
-- Name: feature_types; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.feature_types (
    id character varying(255) NOT NULL,
    name character varying NOT NULL,
    description character varying,
    lifetime_days integer,
    created_at timestamp with time zone DEFAULT now(),
    created_by_user_id integer
);


ALTER TABLE public.feature_types OWNER TO root;

--
-- Name: features; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.features (
    created_at timestamp with time zone DEFAULT now(),
    name character varying(255) NOT NULL,
    description text,
    variants json DEFAULT '[]'::json,
    type character varying DEFAULT 'release'::character varying,
    stale boolean DEFAULT false,
    project character varying DEFAULT 'default'::character varying,
    last_seen_at timestamp with time zone,
    impression_data boolean DEFAULT false,
    archived_at timestamp with time zone,
    potentially_stale boolean,
    created_by_user_id integer,
    archived boolean DEFAULT false
);


ALTER TABLE public.features OWNER TO root;

--
-- Name: users; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.users (
    id integer NOT NULL,
    name character varying(255),
    username character varying(255),
    email character varying(255),
    image_url text,
    password_hash character varying(255),
    login_attempts integer DEFAULT 0,
    created_at timestamp without time zone DEFAULT now(),
    seen_at timestamp without time zone,
    settings json,
    permissions json DEFAULT '[]'::json,
    deleted_at timestamp with time zone,
    is_service boolean DEFAULT false,
    created_by_user_id integer,
    is_system boolean DEFAULT false NOT NULL,
    scim_id text,
    scim_external_id text,
    first_seen_at timestamp without time zone
);


ALTER TABLE public.users OWNER TO root;

--
-- Name: features_view; Type: VIEW; Schema: public; Owner: root
--

CREATE VIEW public.features_view AS
 SELECT features.name,
    features.description,
    features.type,
    features.project,
    features.stale,
    features.impression_data,
    features.created_at,
    features.archived_at,
    features.last_seen_at,
    feature_environments.last_seen_at AS env_last_seen_at,
    feature_environments.enabled,
    feature_environments.environment,
    feature_environments.variants,
    environments.name AS environment_name,
    environments.type AS environment_type,
    environments.sort_order AS environment_sort_order,
    feature_strategies.id AS strategy_id,
    feature_strategies.strategy_name,
    feature_strategies.parameters,
    feature_strategies.constraints,
    feature_strategies.sort_order,
    fss.segment_id AS segments,
    feature_strategies.title AS strategy_title,
    feature_strategies.disabled AS strategy_disabled,
    feature_strategies.variants AS strategy_variants,
    users.id AS user_id,
    users.name AS user_name,
    users.username AS user_username,
    users.email AS user_email
   FROM (((((public.features
     LEFT JOIN public.feature_environments ON (((feature_environments.feature_name)::text = (features.name)::text)))
     LEFT JOIN public.feature_strategies ON ((((feature_strategies.feature_name)::text = (feature_environments.feature_name)::text) AND ((feature_strategies.environment)::text = (feature_environments.environment)::text))))
     LEFT JOIN public.environments ON (((feature_environments.environment)::text = (environments.name)::text)))
     LEFT JOIN public.feature_strategy_segment fss ON ((fss.feature_strategy_id = feature_strategies.id)))
     LEFT JOIN public.users ON ((users.id = features.created_by_user_id)));


ALTER VIEW public.features_view OWNER TO root;

--
-- Name: feedback; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.feedback (
    id integer NOT NULL,
    category text NOT NULL,
    user_type text,
    difficulty_score integer,
    positive text,
    areas_for_improvement text,
    created_at timestamp with time zone DEFAULT now()
);


ALTER TABLE public.feedback OWNER TO root;

--
-- Name: feedback_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.feedback_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.feedback_id_seq OWNER TO root;

--
-- Name: feedback_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.feedback_id_seq OWNED BY public.feedback.id;


--
-- Name: flag_trends; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.flag_trends (
    id character varying(255) NOT NULL,
    project character varying(255) NOT NULL,
    total_flags integer NOT NULL,
    stale_flags integer NOT NULL,
    potentially_stale_flags integer NOT NULL,
    created_at timestamp without time zone DEFAULT now(),
    health integer DEFAULT 100,
    time_to_production double precision DEFAULT 0,
    users integer DEFAULT 0,
    total_yes bigint,
    total_no bigint,
    total_apps integer,
    total_environments integer
);


ALTER TABLE public.flag_trends OWNER TO root;

--
-- Name: group_role; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.group_role (
    group_id integer NOT NULL,
    role_id integer NOT NULL,
    created_by text,
    created_at timestamp with time zone DEFAULT now(),
    project text NOT NULL
);


ALTER TABLE public.group_role OWNER TO root;

--
-- Name: group_user; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.group_user (
    group_id integer NOT NULL,
    user_id integer NOT NULL,
    created_by text,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.group_user OWNER TO root;

--
-- Name: groups; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.groups (
    id integer NOT NULL,
    name text NOT NULL,
    description text,
    created_by text,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    mappings_sso jsonb DEFAULT '[]'::jsonb,
    root_role_id integer,
    scim_id text,
    scim_external_id text
);


ALTER TABLE public.groups OWNER TO root;

--
-- Name: groups_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.groups_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.groups_id_seq OWNER TO root;

--
-- Name: groups_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.groups_id_seq OWNED BY public.groups.id;


--
-- Name: signal_endpoint_tokens; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.signal_endpoint_tokens (
    id integer NOT NULL,
    token text NOT NULL,
    name text NOT NULL,
    signal_endpoint_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    created_by_user_id integer
);


ALTER TABLE public.signal_endpoint_tokens OWNER TO root;

--
-- Name: incoming_webhook_tokens_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.incoming_webhook_tokens_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.incoming_webhook_tokens_id_seq OWNER TO root;

--
-- Name: incoming_webhook_tokens_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.incoming_webhook_tokens_id_seq OWNED BY public.signal_endpoint_tokens.id;


--
-- Name: signal_endpoints; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.signal_endpoints (
    id integer NOT NULL,
    enabled boolean DEFAULT true NOT NULL,
    name text NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    created_by_user_id integer,
    description text
);


ALTER TABLE public.signal_endpoints OWNER TO root;

--
-- Name: incoming_webhooks_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.incoming_webhooks_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.incoming_webhooks_id_seq OWNER TO root;

--
-- Name: incoming_webhooks_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.incoming_webhooks_id_seq OWNED BY public.signal_endpoints.id;


--
-- Name: integration_events; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.integration_events (
    id bigint NOT NULL,
    integration_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    state text NOT NULL,
    state_details text NOT NULL,
    event jsonb NOT NULL,
    details jsonb NOT NULL
);


ALTER TABLE public.integration_events OWNER TO root;

--
-- Name: integration_events_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.integration_events_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.integration_events_id_seq OWNER TO root;

--
-- Name: integration_events_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.integration_events_id_seq OWNED BY public.integration_events.id;


--
-- Name: jobs; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.jobs (
    name text NOT NULL,
    bucket timestamp with time zone NOT NULL,
    stage text NOT NULL,
    finished_at timestamp with time zone
);


ALTER TABLE public.jobs OWNER TO root;

--
-- Name: last_seen_at_metrics; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.last_seen_at_metrics (
    feature_name character varying(255) NOT NULL,
    environment character varying(100) NOT NULL,
    last_seen_at timestamp with time zone NOT NULL
);


ALTER TABLE public.last_seen_at_metrics OWNER TO root;

--
-- Name: login_history; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.login_history (
    id integer NOT NULL,
    username text NOT NULL,
    auth_type text NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    successful boolean NOT NULL,
    ip inet,
    failure_reason text
);


ALTER TABLE public.login_history OWNER TO root;

--
-- Name: login_events_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.login_events_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.login_events_id_seq OWNER TO root;

--
-- Name: login_events_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.login_events_id_seq OWNED BY public.login_history.id;


--
-- Name: message_banners_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.message_banners_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.message_banners_id_seq OWNER TO root;

--
-- Name: message_banners_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.message_banners_id_seq OWNED BY public.banners.id;


--
-- Name: migrations; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.migrations (
    id integer NOT NULL,
    name character varying(255) NOT NULL,
    run_on timestamp without time zone NOT NULL
);


ALTER TABLE public.migrations OWNER TO root;

--
-- Name: migrations_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.migrations_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.migrations_id_seq OWNER TO root;

--
-- Name: migrations_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.migrations_id_seq OWNED BY public.migrations.id;


--
-- Name: notifications; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.notifications (
    id integer NOT NULL,
    event_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.notifications OWNER TO root;

--
-- Name: notifications_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.notifications_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.notifications_id_seq OWNER TO root;

--
-- Name: notifications_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.notifications_id_seq OWNED BY public.notifications.id;


--
-- Name: signals; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.signals (
    id integer NOT NULL,
    payload jsonb DEFAULT '{}'::jsonb NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    source text NOT NULL,
    source_id integer NOT NULL,
    created_by_source_token_id integer,
    announced boolean DEFAULT false NOT NULL
);


ALTER TABLE public.signals OWNER TO root;

--
-- Name: observable_events_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.observable_events_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.observable_events_id_seq OWNER TO root;

--
-- Name: observable_events_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.observable_events_id_seq OWNED BY public.signals.id;


--
-- Name: onboarding_events_instance; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.onboarding_events_instance (
    event character varying(255) NOT NULL,
    time_to_event integer NOT NULL
);


ALTER TABLE public.onboarding_events_instance OWNER TO root;

--
-- Name: onboarding_events_project; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.onboarding_events_project (
    event character varying(255) NOT NULL,
    time_to_event integer NOT NULL,
    project character varying(255) NOT NULL
);


ALTER TABLE public.onboarding_events_project OWNER TO root;

--
-- Name: permissions; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.permissions (
    id integer NOT NULL,
    permission character varying(255) NOT NULL,
    display_name text,
    type character varying(255),
    created_at timestamp with time zone DEFAULT now()
);


ALTER TABLE public.permissions OWNER TO root;

--
-- Name: permissions_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.permissions_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.permissions_id_seq OWNER TO root;

--
-- Name: permissions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.permissions_id_seq OWNED BY public.permissions.id;


--
-- Name: personal_access_tokens; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.personal_access_tokens (
    secret text NOT NULL,
    description text,
    user_id integer NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    seen_at timestamp with time zone,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    id integer NOT NULL
);


ALTER TABLE public.personal_access_tokens OWNER TO root;

--
-- Name: personal_access_tokens_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.personal_access_tokens_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.personal_access_tokens_id_seq OWNER TO root;

--
-- Name: personal_access_tokens_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.personal_access_tokens_id_seq OWNED BY public.personal_access_tokens.id;


--
-- Name: project_client_metrics_trends; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.project_client_metrics_trends (
    project character varying NOT NULL,
    date date NOT NULL,
    total_yes integer NOT NULL,
    total_no integer NOT NULL,
    total_apps integer NOT NULL,
    total_flags integer NOT NULL,
    total_environments integer NOT NULL
);


ALTER TABLE public.project_client_metrics_trends OWNER TO root;

--
-- Name: project_environments; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.project_environments (
    project_id character varying(255) NOT NULL,
    environment_name character varying(100) NOT NULL,
    default_strategy jsonb
);


ALTER TABLE public.project_environments OWNER TO root;

--
-- Name: project_settings; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.project_settings (
    project character varying(255) NOT NULL,
    default_stickiness character varying(100),
    project_mode character varying(100) DEFAULT 'open'::character varying NOT NULL,
    feature_limit integer,
    feature_naming_pattern text,
    feature_naming_example text,
    feature_naming_description text,
    CONSTRAINT project_settings_project_mode_values CHECK (((project_mode)::text = ANY ((ARRAY['open'::character varying, 'protected'::character varying, 'private'::character varying])::text[])))
);


ALTER TABLE public.project_settings OWNER TO root;

--
-- Name: project_stats; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.project_stats (
    project character varying(255) NOT NULL,
    avg_time_to_prod_current_window double precision DEFAULT 0,
    project_changes_current_window integer DEFAULT 0,
    project_changes_past_window integer DEFAULT 0,
    features_created_current_window integer DEFAULT 0,
    features_created_past_window integer DEFAULT 0,
    features_archived_current_window integer DEFAULT 0,
    features_archived_past_window integer DEFAULT 0,
    project_members_added_current_window integer DEFAULT 0
);


ALTER TABLE public.project_stats OWNER TO root;

--
-- Name: projects; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.projects (
    id character varying(255) NOT NULL,
    name character varying NOT NULL,
    description character varying,
    created_at timestamp without time zone DEFAULT now(),
    health integer DEFAULT 100,
    updated_at timestamp with time zone DEFAULT now(),
    archived_at timestamp with time zone
);


ALTER TABLE public.projects OWNER TO root;

--
-- Name: public_signup_tokens; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.public_signup_tokens (
    secret text NOT NULL,
    name text,
    expires_at timestamp with time zone NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    created_by text,
    role_id integer NOT NULL,
    url text,
    enabled boolean DEFAULT true
);


ALTER TABLE public.public_signup_tokens OWNER TO root;

--
-- Name: public_signup_tokens_user; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.public_signup_tokens_user (
    secret text NOT NULL,
    user_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.public_signup_tokens_user OWNER TO root;

--
-- Name: reset_tokens; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.reset_tokens (
    reset_token text NOT NULL,
    user_id integer,
    expires_at timestamp with time zone NOT NULL,
    used_at timestamp with time zone,
    created_at timestamp with time zone DEFAULT now(),
    created_by text
);


ALTER TABLE public.reset_tokens OWNER TO root;

--
-- Name: role_permission; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.role_permission (
    role_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now(),
    environment character varying(100),
    permission text,
    created_by_user_id integer,
    id integer NOT NULL
);


ALTER TABLE public.role_permission OWNER TO root;

--
-- Name: role_permission_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.role_permission_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.role_permission_id_seq OWNER TO root;

--
-- Name: role_permission_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.role_permission_id_seq OWNED BY public.role_permission.id;


--
-- Name: role_user; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.role_user (
    role_id integer NOT NULL,
    user_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now(),
    project character varying(255) NOT NULL,
    created_by_user_id integer
);


ALTER TABLE public.role_user OWNER TO root;

--
-- Name: roles; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.roles (
    id integer NOT NULL,
    name text NOT NULL,
    description text,
    type text DEFAULT 'custom'::text NOT NULL,
    created_at timestamp with time zone DEFAULT now(),
    updated_at timestamp with time zone,
    created_by_user_id integer
);


ALTER TABLE public.roles OWNER TO root;

--
-- Name: roles_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.roles_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.roles_id_seq OWNER TO root;

--
-- Name: roles_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.roles_id_seq OWNED BY public.roles.id;


--
-- Name: segments; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.segments (
    id integer NOT NULL,
    name text NOT NULL,
    description text,
    created_by text,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    constraints jsonb DEFAULT '[]'::jsonb NOT NULL,
    segment_project_id character varying(255)
);


ALTER TABLE public.segments OWNER TO root;

--
-- Name: segments_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.segments_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.segments_id_seq OWNER TO root;

--
-- Name: segments_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.segments_id_seq OWNED BY public.segments.id;


--
-- Name: settings; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.settings (
    name character varying(255) NOT NULL,
    content json
);


ALTER TABLE public.settings OWNER TO root;

--
-- Name: stat_environment_updates; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.stat_environment_updates (
    day date NOT NULL,
    environment text NOT NULL,
    updates bigint DEFAULT 0 NOT NULL
);


ALTER TABLE public.stat_environment_updates OWNER TO root;

--
-- Name: stat_traffic_usage; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.stat_traffic_usage (
    day date NOT NULL,
    traffic_group text NOT NULL,
    status_code_series integer NOT NULL,
    count bigint DEFAULT 0 NOT NULL
);


ALTER TABLE public.stat_traffic_usage OWNER TO root;

--
-- Name: strategies; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.strategies (
    created_at timestamp with time zone DEFAULT now(),
    name character varying(255) NOT NULL,
    description text,
    parameters json,
    built_in integer DEFAULT 0,
    deprecated boolean DEFAULT false,
    sort_order integer DEFAULT 9999,
    display_name text,
    title text
);


ALTER TABLE public.strategies OWNER TO root;

--
-- Name: tag_types; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.tag_types (
    name text NOT NULL,
    description text,
    icon text,
    created_at timestamp with time zone DEFAULT now()
);


ALTER TABLE public.tag_types OWNER TO root;

--
-- Name: tags; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.tags (
    type text NOT NULL,
    value text NOT NULL,
    created_at timestamp with time zone DEFAULT now()
);


ALTER TABLE public.tags OWNER TO root;

--
-- Name: unleash_session; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.unleash_session (
    sid character varying NOT NULL,
    sess json NOT NULL,
    created_at timestamp with time zone DEFAULT now(),
    expired timestamp with time zone NOT NULL
);


ALTER TABLE public.unleash_session OWNER TO root;

--
-- Name: used_passwords; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.used_passwords (
    user_id integer NOT NULL,
    password_hash text NOT NULL,
    used_at timestamp with time zone DEFAULT (now() AT TIME ZONE 'utc'::text)
);


ALTER TABLE public.used_passwords OWNER TO root;

--
-- Name: user_feedback; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.user_feedback (
    user_id integer NOT NULL,
    feedback_id text NOT NULL,
    given timestamp with time zone,
    nevershow boolean DEFAULT false NOT NULL
);


ALTER TABLE public.user_feedback OWNER TO root;

--
-- Name: user_notifications; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.user_notifications (
    notification_id integer NOT NULL,
    user_id integer NOT NULL,
    read_at timestamp with time zone
);


ALTER TABLE public.user_notifications OWNER TO root;

--
-- Name: user_splash; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.user_splash (
    user_id integer NOT NULL,
    splash_id text NOT NULL,
    seen boolean DEFAULT false NOT NULL
);


ALTER TABLE public.user_splash OWNER TO root;

--
-- Name: user_trends; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.user_trends (
    id character varying(255) NOT NULL,
    total_users integer NOT NULL,
    active_users integer NOT NULL,
    created_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.user_trends OWNER TO root;

--
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.users_id_seq OWNER TO root;

--
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- Name: action_set_events id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.action_set_events ALTER COLUMN id SET DEFAULT nextval('public.action_set_events_id_seq'::regclass);


--
-- Name: action_sets id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.action_sets ALTER COLUMN id SET DEFAULT nextval('public.action_sets_id_seq'::regclass);


--
-- Name: actions id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.actions ALTER COLUMN id SET DEFAULT nextval('public.actions_id_seq'::regclass);


--
-- Name: addons id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.addons ALTER COLUMN id SET DEFAULT nextval('public.addons_id_seq'::regclass);


--
-- Name: banners id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.banners ALTER COLUMN id SET DEFAULT nextval('public.message_banners_id_seq'::regclass);


--
-- Name: change_request_approvals id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_approvals ALTER COLUMN id SET DEFAULT nextval('public.change_request_approvals_id_seq'::regclass);


--
-- Name: change_request_comments id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_comments ALTER COLUMN id SET DEFAULT nextval('public.change_request_comments_id_seq'::regclass);


--
-- Name: change_request_events id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_events ALTER COLUMN id SET DEFAULT nextval('public.change_request_events_id_seq'::regclass);


--
-- Name: change_request_rejections id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_rejections ALTER COLUMN id SET DEFAULT nextval('public.change_request_rejections_id_seq'::regclass);


--
-- Name: change_requests id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_requests ALTER COLUMN id SET DEFAULT nextval('public.change_requests_id_seq'::regclass);


--
-- Name: events id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.events ALTER COLUMN id SET DEFAULT nextval('public.events_id_seq'::regclass);


--
-- Name: feedback id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.feedback ALTER COLUMN id SET DEFAULT nextval('public.feedback_id_seq'::regclass);


--
-- Name: groups id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.groups ALTER COLUMN id SET DEFAULT nextval('public.groups_id_seq'::regclass);


--
-- Name: integration_events id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.integration_events ALTER COLUMN id SET DEFAULT nextval('public.integration_events_id_seq'::regclass);


--
-- Name: login_history id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.login_history ALTER COLUMN id SET DEFAULT nextval('public.login_events_id_seq'::regclass);


--
-- Name: migrations id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.migrations ALTER COLUMN id SET DEFAULT nextval('public.migrations_id_seq'::regclass);


--
-- Name: notifications id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.notifications ALTER COLUMN id SET DEFAULT nextval('public.notifications_id_seq'::regclass);


--
-- Name: permissions id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.permissions ALTER COLUMN id SET DEFAULT nextval('public.permissions_id_seq'::regclass);


--
-- Name: personal_access_tokens id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.personal_access_tokens ALTER COLUMN id SET DEFAULT nextval('public.personal_access_tokens_id_seq'::regclass);


--
-- Name: role_permission id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.role_permission ALTER COLUMN id SET DEFAULT nextval('public.role_permission_id_seq'::regclass);


--
-- Name: roles id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.roles ALTER COLUMN id SET DEFAULT nextval('public.roles_id_seq'::regclass);


--
-- Name: segments id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.segments ALTER COLUMN id SET DEFAULT nextval('public.segments_id_seq'::regclass);


--
-- Name: signal_endpoint_tokens id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.signal_endpoint_tokens ALTER COLUMN id SET DEFAULT nextval('public.incoming_webhook_tokens_id_seq'::regclass);


--
-- Name: signal_endpoints id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.signal_endpoints ALTER COLUMN id SET DEFAULT nextval('public.incoming_webhooks_id_seq'::regclass);


--
-- Name: signals id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.signals ALTER COLUMN id SET DEFAULT nextval('public.observable_events_id_seq'::regclass);


--
-- Name: users id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- Data for Name: action_set_events; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: action_sets; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: actions; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: addons; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: api_token_project; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.api_token_project VALUES ('default:production.30d020981e4026e09c4bb07039a58a4abc51a54cd1297e0554e9bffe', 'default');


--
-- Data for Name: api_tokens; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.api_tokens VALUES ('default:production.30d020981e4026e09c4bb07039a58a4abc51a54cd1297e0554e9bffe', 'production', 'client', '2024-10-04 13:39:12.777227+13', NULL, '2024-10-04 13:44:31.217+13', 'production', NULL, 'production', NULL);


--
-- Data for Name: banners; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: change_request_approvals; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: change_request_comments; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: change_request_events; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: change_request_rejections; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: change_request_schedule; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: change_request_settings; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: change_requests; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: client_applications; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.client_applications VALUES ('records', '2024-10-04 13:39:42.675304+13', '2024-10-04 13:39:42.674+13', '2024-10-04 13:39:42.674+13', '["default","userWithId","gradualRolloutUserId","gradualRolloutRandom","applicationHostname","gradualRolloutSessionId","remoteAddress","flexibleRollout"]', NULL, NULL, NULL, NULL, true, 'unleash_system_user');


--
-- Data for Name: client_applications_usage; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.client_applications_usage VALUES ('records', 'default', 'production');


--
-- Data for Name: client_instances; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.client_instances VALUES ('records', 'DESKTOP-K2RQV8G-generated-8e07d6b2-e3bc-4f53-a618-ec1a09063fcc', '::ffff:172.19.0.1', '2024-10-04 13:44:41.047+13', '2024-10-04 13:39:42.683377+13', 'unleash-client-dotnet:v4.1.13', 'production');


--
-- Data for Name: client_metrics_env; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.client_metrics_env VALUES ('records_JwtCreate', 'records', 'production', '2024-10-04 13:00:00+13', 5, 36);


--
-- Data for Name: client_metrics_env_daily; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: client_metrics_env_variants; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: client_metrics_env_variants_daily; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: context_fields; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.context_fields VALUES ('environment', 'Allows you to constrain on application environment', 0, NULL, '2024-10-04 13:36:35.272479', '2024-10-04 13:36:35.272479', false);
INSERT INTO public.context_fields VALUES ('userId', 'Allows you to constrain on userId', 1, NULL, '2024-10-04 13:36:35.272479', '2024-10-04 13:36:35.272479', false);
INSERT INTO public.context_fields VALUES ('appName', 'Allows you to constrain on application name', 2, NULL, '2024-10-04 13:36:35.272479', '2024-10-04 13:36:35.272479', false);
INSERT INTO public.context_fields VALUES ('currentTime', 'Allows you to constrain on date values', 3, NULL, '2024-10-04 13:36:36.01752', '2024-10-04 13:36:36.01752', false);
INSERT INTO public.context_fields VALUES ('sessionId', 'Allows you to constrain on sessionId', 4, NULL, '2024-10-04 13:36:36.436474', '2024-10-04 13:36:36.436474', true);


--
-- Data for Name: dependent_features; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: environment_type_trends; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: environments; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.environments VALUES ('default', '2024-10-04 13:36:35.750246+13', 1, 'production', false, true);
INSERT INTO public.environments VALUES ('development', '2024-10-04 13:36:35.785099+13', 2, 'development', true, false);
INSERT INTO public.environments VALUES ('production', '2024-10-04 13:36:35.785099+13', 3, 'production', true, false);


--
-- Data for Name: events; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.events VALUES (1, '2024-10-04 13:36:35.005954+13', 'strategy-created', 'migration', '{"name":"default","description":"Default on or off Strategy."}', '[]', NULL, NULL, NULL, NULL, true, NULL, NULL);
INSERT INTO public.events VALUES (2, '2024-10-04 13:36:35.087127+13', 'strategy-created', 'migration', '{"name":"userWithId","description":"Active for users with a userId defined in the userIds-list","parameters":[{"name":"userIds","type":"list","description":"","required":false}]}', '[]', NULL, NULL, NULL, NULL, true, NULL, NULL);
INSERT INTO public.events VALUES (3, '2024-10-04 13:36:35.087127+13', 'strategy-created', 'migration', '{"name":"applicationHostname","description":"Active for client instances with a hostName in the hostNames-list.","parameters":[{"name":"hostNames","type":"list","description":"List of hostnames to enable the feature toggle for.","required":false}]}', '[]', NULL, NULL, NULL, NULL, true, NULL, NULL);
INSERT INTO public.events VALUES (4, '2024-10-04 13:36:35.087127+13', 'strategy-created', 'migration', '{"name":"remoteAddress","description":"Active for remote addresses defined in the IPs list.","parameters":[{"name":"IPs","type":"list","description":"List of IPs to enable the feature toggle for.","required":true}]}', '[]', NULL, NULL, NULL, NULL, true, NULL, NULL);
INSERT INTO public.events VALUES (5, '2024-10-04 13:36:35.266242+13', 'strategy-created', 'migration', '{"name":"flexibleRollout","description":"Gradually activate feature toggle based on sane stickiness","parameters":[{"name":"rollout","type":"percentage","description":"","required":false},{"name":"stickiness","type":"string","description":"Used define stickiness. Possible values: default, userId, sessionId, random","required":true},{"name":"groupId","type":"string","description":"Used to define a activation groups, which allows you to correlate across feature toggles.","required":true}]}', '[]', NULL, NULL, NULL, NULL, true, NULL, NULL);
INSERT INTO public.events VALUES (6, '2024-10-04 13:37:21.659595+13', 'feature-created', 'admin', '{"name":"records_UserCreate","description":null,"type":"kill-switch","project":"default","stale":false,"createdAt":"2024-10-04T00:37:21.650Z","lastSeenAt":null,"impressionData":false,"archivedAt":null,"archived":false}', '[]', 'default', NULL, 'records_UserCreate', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (7, '2024-10-04 13:37:23.704321+13', 'feature-strategy-add', 'admin', '{"id":"335657a0-e9f3-4162-9cb4-61786f6b76c5","name":"flexibleRollout","title":null,"disabled":false,"constraints":[],"parameters":{"groupId":"records_UserCreate","rollout":"100","stickiness":"default"},"variants":[],"sortOrder":0,"segments":[]}', '[]', 'default', 'development', 'records_UserCreate', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (8, '2024-10-04 13:37:23.711138+13', 'feature-environment-enabled', 'admin', NULL, '[]', 'default', 'development', 'records_UserCreate', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (9, '2024-10-04 13:37:25.35611+13', 'feature-strategy-add', 'admin', '{"id":"8358aaf3-f447-4f08-8c46-e5845f12e734","name":"flexibleRollout","title":null,"disabled":false,"constraints":[],"parameters":{"groupId":"records_UserCreate","rollout":"100","stickiness":"default"},"variants":[],"sortOrder":0,"segments":[]}', '[]', 'default', 'production', 'records_UserCreate', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (10, '2024-10-04 13:37:25.365452+13', 'feature-environment-enabled', 'admin', NULL, '[]', 'default', 'production', 'records_UserCreate', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (11, '2024-10-04 13:38:03.117667+13', 'feature-created', 'admin', '{"name":"records_JwtCreate","description":null,"type":"kill-switch","project":"default","stale":false,"createdAt":"2024-10-04T00:38:03.111Z","lastSeenAt":null,"impressionData":false,"archivedAt":null,"archived":false}', '[]', 'default', NULL, 'records_JwtCreate', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (12, '2024-10-04 13:38:04.461201+13', 'feature-strategy-add', 'admin', '{"id":"17f683e9-3d19-42c0-b335-feb5a295be4b","name":"flexibleRollout","title":null,"disabled":false,"constraints":[],"parameters":{"groupId":"records_JwtCreate","rollout":"100","stickiness":"default"},"variants":[],"sortOrder":0,"segments":[]}', '[]', 'default', 'development', 'records_JwtCreate', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (13, '2024-10-04 13:38:04.465717+13', 'feature-environment-enabled', 'admin', NULL, '[]', 'default', 'development', 'records_JwtCreate', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (14, '2024-10-04 13:38:06.895902+13', 'feature-strategy-add', 'admin', '{"id":"5802bc03-237e-44f9-8e89-df920d2950b1","name":"flexibleRollout","title":null,"disabled":false,"constraints":[],"parameters":{"groupId":"records_JwtCreate","rollout":"100","stickiness":"default"},"variants":[],"sortOrder":0,"segments":[]}', '[]', 'default', 'production', 'records_JwtCreate', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (15, '2024-10-04 13:38:06.900229+13', 'feature-environment-enabled', 'admin', NULL, '[]', 'default', 'production', 'records_JwtCreate', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (16, '2024-10-04 13:38:35.910529+13', 'feature-created', 'admin', '{"name":"records_tests_FeatureEnabled","description":null,"type":"kill-switch","project":"default","stale":false,"createdAt":"2024-10-04T00:38:35.904Z","lastSeenAt":null,"impressionData":false,"archivedAt":null,"archived":false}', '[]', 'default', NULL, 'records_tests_FeatureEnabled', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (17, '2024-10-04 13:38:37.535435+13', 'feature-strategy-add', 'admin', '{"id":"d34b2bb1-1b17-44c8-9e79-46438c625a7f","name":"flexibleRollout","title":null,"disabled":false,"constraints":[],"parameters":{"groupId":"records_tests_FeatureEnabled","rollout":"100","stickiness":"default"},"variants":[],"sortOrder":0,"segments":[]}', '[]', 'default', 'development', 'records_tests_FeatureEnabled', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (18, '2024-10-04 13:38:37.539634+13', 'feature-environment-enabled', 'admin', NULL, '[]', 'default', 'development', 'records_tests_FeatureEnabled', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (19, '2024-10-04 13:38:38.99897+13', 'feature-strategy-add', 'admin', '{"id":"f684e6c3-7563-44bf-a288-3204d0c3cbb0","name":"flexibleRollout","title":null,"disabled":false,"constraints":[],"parameters":{"groupId":"records_tests_FeatureEnabled","rollout":"100","stickiness":"default"},"variants":[],"sortOrder":0,"segments":[]}', '[]', 'default', 'production', 'records_tests_FeatureEnabled', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (20, '2024-10-04 13:38:39.002855+13', 'feature-environment-enabled', 'admin', NULL, '[]', 'default', 'production', 'records_tests_FeatureEnabled', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (21, '2024-10-04 13:39:00.706081+13', 'feature-created', 'admin', '{"name":"records_test_FeatureDisabled","description":null,"type":"release","project":"default","stale":false,"createdAt":"2024-10-04T00:39:00.699Z","lastSeenAt":null,"impressionData":false,"archivedAt":null,"archived":false}', '[]', 'default', NULL, 'records_test_FeatureDisabled', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (22, '2024-10-04 13:39:12.782797+13', 'api-token-created', 'admin', '{"tokenName":"production","environment":"production","projects":["default"],"type":"client","username":"production","alias":null,"project":"default","createdAt":"2024-10-04T00:39:12.777Z"}', '[]', 'default', 'production', NULL, NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (23, '2024-10-04 13:39:57.781054+13', 'feature-environment-disabled', 'admin', NULL, '[]', 'default', 'production', 'records_JwtCreate', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (24, '2024-10-04 13:40:17.167674+13', 'feature-environment-enabled', 'admin', NULL, '[]', 'default', 'production', 'records_JwtCreate', NULL, true, 1, '::ffff:172.19.0.1');
INSERT INTO public.events VALUES (25, '2024-10-04 13:41:46.329048+13', 'application-created', 'unleash_system_user', '{"appName":"records","createdAt":"2024-10-04T00:39:42.675Z","updatedAt":"2024-10-04T00:39:42.674Z","description":null,"strategies":["default","userWithId","gradualRolloutUserId","gradualRolloutRandom","applicationHostname","gradualRolloutSessionId","remoteAddress","flexibleRollout"],"createdBy":"unleash_system_user","url":null,"color":null,"icon":null}', '[]', NULL, NULL, NULL, NULL, true, -1337, '');


--
-- Data for Name: favorite_features; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: favorite_projects; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: feature_environments; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.feature_environments VALUES ('development', 'records_UserCreate', true, '[]', NULL);
INSERT INTO public.feature_environments VALUES ('production', 'records_UserCreate', true, '[]', NULL);
INSERT INTO public.feature_environments VALUES ('development', 'records_JwtCreate', true, '[]', NULL);
INSERT INTO public.feature_environments VALUES ('development', 'records_tests_FeatureEnabled', true, '[]', NULL);
INSERT INTO public.feature_environments VALUES ('production', 'records_tests_FeatureEnabled', true, '[]', NULL);
INSERT INTO public.feature_environments VALUES ('development', 'records_test_FeatureDisabled', false, '[]', NULL);
INSERT INTO public.feature_environments VALUES ('production', 'records_test_FeatureDisabled', false, '[]', NULL);
INSERT INTO public.feature_environments VALUES ('production', 'records_JwtCreate', true, '[]', NULL);


--
-- Data for Name: feature_lifecycles; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.feature_lifecycles VALUES ('records_UserCreate', 'initial', '2024-10-04 13:37:21.683+13', NULL, NULL);
INSERT INTO public.feature_lifecycles VALUES ('records_JwtCreate', 'initial', '2024-10-04 13:38:03.697+13', NULL, NULL);
INSERT INTO public.feature_lifecycles VALUES ('records_tests_FeatureEnabled', 'initial', '2024-10-04 13:38:36.701+13', NULL, NULL);
INSERT INTO public.feature_lifecycles VALUES ('records_test_FeatureDisabled', 'initial', '2024-10-04 13:39:01.704+13', NULL, NULL);
INSERT INTO public.feature_lifecycles VALUES ('records_JwtCreate', 'pre-live', '2024-10-04 13:40:42.683+13', NULL, NULL);
INSERT INTO public.feature_lifecycles VALUES ('records_JwtCreate', 'live', '2024-10-04 13:40:42.685+13', NULL, NULL);


--
-- Data for Name: feature_strategies; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.feature_strategies VALUES ('335657a0-e9f3-4162-9cb4-61786f6b76c5', 'records_UserCreate', 'default', 'development', 'flexibleRollout', '{"groupId": "records_UserCreate", "rollout": "100", "stickiness": "default"}', '[]', 0, '2024-10-04 13:37:23.698024+13', NULL, false, '[]', NULL);
INSERT INTO public.feature_strategies VALUES ('8358aaf3-f447-4f08-8c46-e5845f12e734', 'records_UserCreate', 'default', 'production', 'flexibleRollout', '{"groupId": "records_UserCreate", "rollout": "100", "stickiness": "default"}', '[]', 0, '2024-10-04 13:37:25.353467+13', NULL, false, '[]', NULL);
INSERT INTO public.feature_strategies VALUES ('17f683e9-3d19-42c0-b335-feb5a295be4b', 'records_JwtCreate', 'default', 'development', 'flexibleRollout', '{"groupId": "records_JwtCreate", "rollout": "100", "stickiness": "default"}', '[]', 0, '2024-10-04 13:38:04.45822+13', NULL, false, '[]', NULL);
INSERT INTO public.feature_strategies VALUES ('5802bc03-237e-44f9-8e89-df920d2950b1', 'records_JwtCreate', 'default', 'production', 'flexibleRollout', '{"groupId": "records_JwtCreate", "rollout": "100", "stickiness": "default"}', '[]', 0, '2024-10-04 13:38:06.892832+13', NULL, false, '[]', NULL);
INSERT INTO public.feature_strategies VALUES ('d34b2bb1-1b17-44c8-9e79-46438c625a7f', 'records_tests_FeatureEnabled', 'default', 'development', 'flexibleRollout', '{"groupId": "records_tests_FeatureEnabled", "rollout": "100", "stickiness": "default"}', '[]', 0, '2024-10-04 13:38:37.532746+13', NULL, false, '[]', NULL);
INSERT INTO public.feature_strategies VALUES ('f684e6c3-7563-44bf-a288-3204d0c3cbb0', 'records_tests_FeatureEnabled', 'default', 'production', 'flexibleRollout', '{"groupId": "records_tests_FeatureEnabled", "rollout": "100", "stickiness": "default"}', '[]', 0, '2024-10-04 13:38:38.995609+13', NULL, false, '[]', NULL);


--
-- Data for Name: feature_strategy_segment; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: feature_tag; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: feature_types; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.feature_types VALUES ('release', 'Release', 'Release feature toggles are used to release new features.', 40, '2024-10-04 13:36:35.35023+13', NULL);
INSERT INTO public.feature_types VALUES ('experiment', 'Experiment', 'Experiment feature toggles are used to test and verify multiple different versions of a feature.', 40, '2024-10-04 13:36:35.35023+13', NULL);
INSERT INTO public.feature_types VALUES ('operational', 'Operational', 'Operational feature toggles are used to control aspects of a rollout.', 7, '2024-10-04 13:36:35.35023+13', NULL);
INSERT INTO public.feature_types VALUES ('kill-switch', 'Kill switch', 'Kill switch feature toggles are used to quickly turn on or off critical functionality in your system.', NULL, '2024-10-04 13:36:35.35023+13', NULL);
INSERT INTO public.feature_types VALUES ('permission', 'Permission', 'Permission feature toggles are used to control permissions in your system.', NULL, '2024-10-04 13:36:35.35023+13', NULL);


--
-- Data for Name: features; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.features VALUES ('2024-10-04 13:37:21.650419+13', 'records_UserCreate', NULL, '[]', 'kill-switch', false, 'default', NULL, false, NULL, NULL, 1, false);
INSERT INTO public.features VALUES ('2024-10-04 13:38:03.111143+13', 'records_JwtCreate', NULL, '[]', 'kill-switch', false, 'default', NULL, false, NULL, NULL, 1, false);
INSERT INTO public.features VALUES ('2024-10-04 13:38:35.904828+13', 'records_tests_FeatureEnabled', NULL, '[]', 'kill-switch', false, 'default', NULL, false, NULL, NULL, 1, false);
INSERT INTO public.features VALUES ('2024-10-04 13:39:00.699537+13', 'records_test_FeatureDisabled', NULL, '[]', 'release', false, 'default', NULL, false, NULL, NULL, 1, false);


--
-- Data for Name: feedback; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: flag_trends; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: group_role; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: group_user; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: groups; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: integration_events; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: jobs; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: last_seen_at_metrics; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.last_seen_at_metrics VALUES ('records_JwtCreate', 'production', '2024-10-04 13:41:49.004+13');


--
-- Data for Name: login_history; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: migrations; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.migrations VALUES (1, '/20141020151056-initial-schema', '2024-10-04 13:36:34.991');
INSERT INTO public.migrations VALUES (2, '/20141110144153-add-description-to-features', '2024-10-04 13:36:34.997');
INSERT INTO public.migrations VALUES (3, '/20141117200435-add-parameters-template-to-strategies', '2024-10-04 13:36:34.999');
INSERT INTO public.migrations VALUES (4, '/20141117202209-insert-default-strategy', '2024-10-04 13:36:35.004');
INSERT INTO public.migrations VALUES (5, '/20141118071458-default-strategy-event', '2024-10-04 13:36:35.008');
INSERT INTO public.migrations VALUES (6, '/20141215210141-005-archived-flag-to-features', '2024-10-04 13:36:35.01');
INSERT INTO public.migrations VALUES (7, '/20150210152531-006-rename-eventtype', '2024-10-04 13:36:35.018');
INSERT INTO public.migrations VALUES (8, '/20160618193924-add-strategies-to-features', '2024-10-04 13:36:35.026');
INSERT INTO public.migrations VALUES (9, '/20161027134128-create-metrics', '2024-10-04 13:36:35.039');
INSERT INTO public.migrations VALUES (10, '/20161104074441-create-client-instances', '2024-10-04 13:36:35.048');
INSERT INTO public.migrations VALUES (11, '/20161205203516-create-client-applications', '2024-10-04 13:36:35.06');
INSERT INTO public.migrations VALUES (12, '/20161212101749-better-strategy-parameter-definitions', '2024-10-04 13:36:35.079');
INSERT INTO public.migrations VALUES (13, '/20170211085502-built-in-strategies', '2024-10-04 13:36:35.084');
INSERT INTO public.migrations VALUES (14, '/20170211090541-add-default-strategies', '2024-10-04 13:36:35.093');
INSERT INTO public.migrations VALUES (15, '/20170306233934-timestamp-with-tz', '2024-10-04 13:36:35.209');
INSERT INTO public.migrations VALUES (16, '/20170628205541-add-sdk-version-to-client-instances', '2024-10-04 13:36:35.262');
INSERT INTO public.migrations VALUES (17, '/20190123204125-add-variants-to-features', '2024-10-04 13:36:35.264');
INSERT INTO public.migrations VALUES (18, '/20191023184858-flexible-rollout-strategy', '2024-10-04 13:36:35.27');
INSERT INTO public.migrations VALUES (19, '/20200102184820-create-context-fields', '2024-10-04 13:36:35.286');
INSERT INTO public.migrations VALUES (20, '/20200227202711-settings', '2024-10-04 13:36:35.299');
INSERT INTO public.migrations VALUES (21, '/20200329191251-settings-secret', '2024-10-04 13:36:35.304');
INSERT INTO public.migrations VALUES (22, '/20200416201319-create-users', '2024-10-04 13:36:35.323');
INSERT INTO public.migrations VALUES (23, '/20200429175747-users-settings', '2024-10-04 13:36:35.326');
INSERT INTO public.migrations VALUES (24, '/20200805091409-add-feature-toggle-type', '2024-10-04 13:36:35.342');
INSERT INTO public.migrations VALUES (25, '/20200805094311-add-feature-type-to-features', '2024-10-04 13:36:35.346');
INSERT INTO public.migrations VALUES (26, '/20200806091734-add-stale-flag-to-features', '2024-10-04 13:36:35.348');
INSERT INTO public.migrations VALUES (27, '/20200810200901-add-created-at-to-feature-types', '2024-10-04 13:36:35.35');
INSERT INTO public.migrations VALUES (28, '/20200928194947-add-projects', '2024-10-04 13:36:35.366');
INSERT INTO public.migrations VALUES (29, '/20200928195238-add-project-id-to-features', '2024-10-04 13:36:35.37');
INSERT INTO public.migrations VALUES (30, '/20201216140726-add-last-seen-to-features', '2024-10-04 13:36:35.372');
INSERT INTO public.migrations VALUES (31, '/20210105083014-add-tag-and-tag-types', '2024-10-04 13:36:35.408');
INSERT INTO public.migrations VALUES (32, '/20210119084617-add-addon-table', '2024-10-04 13:36:35.427');
INSERT INTO public.migrations VALUES (33, '/20210121115438-add-deprecated-column-to-strategies', '2024-10-04 13:36:35.431');
INSERT INTO public.migrations VALUES (34, '/20210127094440-add-tags-column-to-events', '2024-10-04 13:36:35.434');
INSERT INTO public.migrations VALUES (35, '/20210208203708-add-stickiness-to-context', '2024-10-04 13:36:35.436');
INSERT INTO public.migrations VALUES (36, '/20210212114759-add-session-table', '2024-10-04 13:36:35.451');
INSERT INTO public.migrations VALUES (37, '/20210217195834-rbac-tables', '2024-10-04 13:36:35.48');
INSERT INTO public.migrations VALUES (38, '/20210218090213-generate-server-identifier', '2024-10-04 13:36:35.484');
INSERT INTO public.migrations VALUES (39, '/20210302080040-add-pk-to-client-instances', '2024-10-04 13:36:35.492');
INSERT INTO public.migrations VALUES (40, '/20210304115810-change-default-timestamp-to-now', '2024-10-04 13:36:35.495');
INSERT INTO public.migrations VALUES (41, '/20210304141005-add-announce-field-to-application', '2024-10-04 13:36:35.499');
INSERT INTO public.migrations VALUES (42, '/20210304150739-add-created-by-to-application', '2024-10-04 13:36:35.501');
INSERT INTO public.migrations VALUES (43, '/20210322104356-api-tokens-table', '2024-10-04 13:36:35.512');
INSERT INTO public.migrations VALUES (44, '/20210322104357-api-tokens-convert-enterprise', '2024-10-04 13:36:35.515');
INSERT INTO public.migrations VALUES (45, '/20210323073508-reset-application-announcements', '2024-10-04 13:36:35.518');
INSERT INTO public.migrations VALUES (46, '/20210409120136-create-reset-token-table', '2024-10-04 13:36:35.529');
INSERT INTO public.migrations VALUES (47, '/20210414141220-fix-misspellings-in-role-descriptions', '2024-10-04 13:36:35.533');
INSERT INTO public.migrations VALUES (48, '/20210415173116-rbac-rename-roles', '2024-10-04 13:36:35.535');
INSERT INTO public.migrations VALUES (49, '/20210421133845-add-sort-order-to-strategies', '2024-10-04 13:36:35.538');
INSERT INTO public.migrations VALUES (50, '/20210421135405-add-display-name-and-update-description-for-strategies', '2024-10-04 13:36:35.542');
INSERT INTO public.migrations VALUES (51, '/20210423103647-lowercase-all-emails', '2024-10-04 13:36:35.548');
INSERT INTO public.migrations VALUES (52, '/20210428062103-user-permission-to-rbac', '2024-10-04 13:36:35.551');
INSERT INTO public.migrations VALUES (53, '/20210428103922-patch-role-table', '2024-10-04 13:36:35.554');
INSERT INTO public.migrations VALUES (54, '/20210428103923-onboard-projects-to-rbac', '2024-10-04 13:36:35.559');
INSERT INTO public.migrations VALUES (55, '/20210428103924-patch-admin-role-name', '2024-10-04 13:36:35.561');
INSERT INTO public.migrations VALUES (56, '/20210428103924-patch-admin_role', '2024-10-04 13:36:35.566');
INSERT INTO public.migrations VALUES (57, '/20210428103924-patch-role_permissions', '2024-10-04 13:36:35.569');
INSERT INTO public.migrations VALUES (58, '/20210504101429-deprecate-strategies', '2024-10-04 13:36:35.572');
INSERT INTO public.migrations VALUES (59, '/20210520171325-update-role-descriptions', '2024-10-04 13:36:35.574');
INSERT INTO public.migrations VALUES (60, '/20210602115555-create-feedback-table', '2024-10-04 13:36:35.59');
INSERT INTO public.migrations VALUES (61, '/20210610085817-features-strategies-table', '2024-10-04 13:36:35.617');
INSERT INTO public.migrations VALUES (62, '/20210615115226-migrate-strategies-to-feature-strategies', '2024-10-04 13:36:35.622');
INSERT INTO public.migrations VALUES (63, '/20210618091331-project-environments-table', '2024-10-04 13:36:35.632');
INSERT INTO public.migrations VALUES (64, '/20210618100913-add-cascade-for-user-feedback', '2024-10-04 13:36:35.638');
INSERT INTO public.migrations VALUES (65, '/20210624114602-change-type-of-feature-archived', '2024-10-04 13:36:35.652');
INSERT INTO public.migrations VALUES (66, '/20210624114855-drop-strategies-column-from-features', '2024-10-04 13:36:35.661');
INSERT INTO public.migrations VALUES (67, '/20210624115109-drop-enabled-column-from-features', '2024-10-04 13:36:35.664');
INSERT INTO public.migrations VALUES (68, '/20210625102126-connect-default-project-to-global-environment', '2024-10-04 13:36:35.67');
INSERT INTO public.migrations VALUES (69, '/20210629130734-add-health-rating-to-project', '2024-10-04 13:36:35.673');
INSERT INTO public.migrations VALUES (70, '/20210830113948-connect-projects-to-global-envrionments', '2024-10-04 13:36:35.676');
INSERT INTO public.migrations VALUES (71, '/20210831072631-add-sort-order-and-type-to-env', '2024-10-04 13:36:35.684');
INSERT INTO public.migrations VALUES (72, '/20210907124058-add-dbcritic-indices', '2024-10-04 13:36:35.719');
INSERT INTO public.migrations VALUES (73, '/20210907124850-add-dbcritic-primary-keys', '2024-10-04 13:36:35.726');
INSERT INTO public.migrations VALUES (74, '/20210908100701-add-enabled-to-environments', '2024-10-04 13:36:35.731');
INSERT INTO public.migrations VALUES (75, '/20210909085651-add-protected-field-to-environments', '2024-10-04 13:36:35.735');
INSERT INTO public.migrations VALUES (76, '/20210913103159-api-keys-scoping', '2024-10-04 13:36:35.738');
INSERT INTO public.migrations VALUES (77, '/20210915122001-add-project-and-environment-columns-to-events', '2024-10-04 13:36:35.748');
INSERT INTO public.migrations VALUES (78, '/20210920104218-rename-global-env-to-default-env', '2024-10-04 13:36:35.762');
INSERT INTO public.migrations VALUES (79, '/20210921105032-client-api-tokens-default', '2024-10-04 13:36:35.769');
INSERT INTO public.migrations VALUES (80, '/20210922084509-add-non-null-constraint-to-environment-type', '2024-10-04 13:36:35.776');
INSERT INTO public.migrations VALUES (81, '/20210922120521-add-tag-type-permission', '2024-10-04 13:36:35.78');
INSERT INTO public.migrations VALUES (82, '/20210928065411-remove-displayname-from-environments', '2024-10-04 13:36:35.783');
INSERT INTO public.migrations VALUES (83, '/20210928080601-add-development-and-production-environments', '2024-10-04 13:36:35.786');
INSERT INTO public.migrations VALUES (84, '/20210928082228-connect-default-environment-to-all-existing-projects', '2024-10-04 13:36:35.789');
INSERT INTO public.migrations VALUES (85, '/20211004104917-client-metrics-env', '2024-10-04 13:36:35.805');
INSERT INTO public.migrations VALUES (86, '/20211011094226-add-environment-to-client-instances', '2024-10-04 13:36:35.815');
INSERT INTO public.migrations VALUES (87, '/20211013093114-feature-strategies-parameters-not-null', '2024-10-04 13:36:35.822');
INSERT INTO public.migrations VALUES (88, '/20211029094324-set-sort-order-env', '2024-10-04 13:36:35.824');
INSERT INTO public.migrations VALUES (89, '/20211105104316-add-feature-name-column-to-events', '2024-10-04 13:36:35.83');
INSERT INTO public.migrations VALUES (90, '/20211105105509-add-predata-column-to-events', '2024-10-04 13:36:35.832');
INSERT INTO public.migrations VALUES (91, '/20211108130333-create-user-splash-table', '2024-10-04 13:36:35.846');
INSERT INTO public.migrations VALUES (92, '/20211109103930-add-splash-entry-for-users', '2024-10-04 13:36:35.849');
INSERT INTO public.migrations VALUES (93, '/20211126112551-disable-default-environment', '2024-10-04 13:36:35.853');
INSERT INTO public.migrations VALUES (94, '/20211130142314-add-updated-at-to-projects', '2024-10-04 13:36:35.856');
INSERT INTO public.migrations VALUES (95, '/20211202120808-add-custom-roles', '2024-10-04 13:36:35.902');
INSERT INTO public.migrations VALUES (96, '/20211209205201-drop-client-metrics', '2024-10-04 13:36:35.92');
INSERT INTO public.migrations VALUES (97, '/20220103134659-add-permissions-to-project-roles', '2024-10-04 13:36:35.935');
INSERT INTO public.migrations VALUES (98, '/20220103143843-add-permissions-to-editor-role', '2024-10-04 13:36:35.938');
INSERT INTO public.migrations VALUES (99, '/20220111112804-update-permission-descriptions', '2024-10-04 13:36:35.94');
INSERT INTO public.migrations VALUES (100, '/20220111115613-move-feature-toggle-permission', '2024-10-04 13:36:35.943');
INSERT INTO public.migrations VALUES (101, '/20220111120346-roles-unique-name', '2024-10-04 13:36:35.949');
INSERT INTO public.migrations VALUES (102, '/20220111121010-update-project-for-editor-role', '2024-10-04 13:36:35.954');
INSERT INTO public.migrations VALUES (103, '/20220111125620-role-permission-empty-string-for-non-environment-type', '2024-10-04 13:36:35.959');
INSERT INTO public.migrations VALUES (104, '/20220119182603-update-toggle-types-description', '2024-10-04 13:36:35.965');
INSERT INTO public.migrations VALUES (105, '/20220125200908-convert-old-feature-events', '2024-10-04 13:36:35.974');
INSERT INTO public.migrations VALUES (106, '/20220128081242-add-impressiondata-to-features', '2024-10-04 13:36:35.978');
INSERT INTO public.migrations VALUES (107, '/20220129113106-metrics-counters-as-bigint', '2024-10-04 13:36:36');
INSERT INTO public.migrations VALUES (108, '/20220131082150-reset-feedback-form', '2024-10-04 13:36:36.013');
INSERT INTO public.migrations VALUES (109, '/20220224081422-remove-project-column-from-roles', '2024-10-04 13:36:36.015');
INSERT INTO public.migrations VALUES (110, '/20220224111626-add-current-time-context-field', '2024-10-04 13:36:36.018');
INSERT INTO public.migrations VALUES (111, '/20220307130902-add-segments', '2024-10-04 13:36:36.045');
INSERT INTO public.migrations VALUES (112, '/20220331085057-add-api-link-table', '2024-10-04 13:36:36.057');
INSERT INTO public.migrations VALUES (113, '/20220405103233-add-segments-name-index', '2024-10-04 13:36:36.062');
INSERT INTO public.migrations VALUES (114, '/20220408081222-clean-up-duplicate-foreign-key-role-permission', '2024-10-04 13:36:36.065');
INSERT INTO public.migrations VALUES (115, '/20220411103724-add-legal-value-description', '2024-10-04 13:36:36.087');
INSERT INTO public.migrations VALUES (116, '/20220425090847-add-token-permission', '2024-10-04 13:36:36.099');
INSERT INTO public.migrations VALUES (117, '/20220511111823-patch-broken-feature-strategies', '2024-10-04 13:36:36.103');
INSERT INTO public.migrations VALUES (118, '/20220511124923-fix-patch-broken-feature-strategies', '2024-10-04 13:36:36.106');
INSERT INTO public.migrations VALUES (119, '/20220528143630-dont-cascade-environment-deletion-to-apitokens', '2024-10-04 13:36:36.108');
INSERT INTO public.migrations VALUES (120, '/20220603081324-add-archive-at-to-feature-toggle', '2024-10-04 13:36:36.112');
INSERT INTO public.migrations VALUES (121, '/20220704115624-add-user-groups', '2024-10-04 13:36:36.142');
INSERT INTO public.migrations VALUES (122, '/20220711084613-add-projects-and-environments-for-addons', '2024-10-04 13:36:36.147');
INSERT INTO public.migrations VALUES (123, '/20220808084524-add-group-permissions', '2024-10-04 13:36:36.15');
INSERT INTO public.migrations VALUES (124, '/20220808110415-add-projects-foreign-key', '2024-10-04 13:36:36.155');
INSERT INTO public.migrations VALUES (125, '/20220816121136-add-metadata-to-api-keys', '2024-10-04 13:36:36.158');
INSERT INTO public.migrations VALUES (126, '/20220817130250-alter-api-tokens', '2024-10-04 13:36:36.16');
INSERT INTO public.migrations VALUES (127, '/20220908093515-add-public-signup-tokens', '2024-10-04 13:36:36.178');
INSERT INTO public.migrations VALUES (128, '/20220912165344-pat-tokens', '2024-10-04 13:36:36.19');
INSERT INTO public.migrations VALUES (129, '/20220916093515-add-url-to-public-signup-tokens', '2024-10-04 13:36:36.192');
INSERT INTO public.migrations VALUES (130, '/20220927110212-add-enabled-to-public-signup-tokens', '2024-10-04 13:36:36.195');
INSERT INTO public.migrations VALUES (131, '/20221010114644-pat-auto-increment', '2024-10-04 13:36:36.208');
INSERT INTO public.migrations VALUES (132, '/20221011155007-add-user-groups-mappings', '2024-10-04 13:36:36.218');
INSERT INTO public.migrations VALUES (133, '/20221103111940-fix-migrations', '2024-10-04 13:36:36.22');
INSERT INTO public.migrations VALUES (134, '/20221103112200-change-request', '2024-10-04 13:36:36.244');
INSERT INTO public.migrations VALUES (135, '/20221103125732-change-request-remove-unique', '2024-10-04 13:36:36.248');
INSERT INTO public.migrations VALUES (136, '/20221104123349-change-request-approval', '2024-10-04 13:36:36.259');
INSERT INTO public.migrations VALUES (137, '/20221107121635-move-variants-to-per-environment', '2024-10-04 13:36:36.273');
INSERT INTO public.migrations VALUES (138, '/20221107132528-change-request-project-options', '2024-10-04 13:36:36.276');
INSERT INTO public.migrations VALUES (139, '/20221108114358-add-change-request-permissions', '2024-10-04 13:36:36.278');
INSERT INTO public.migrations VALUES (140, '/20221110104933-add-change-request-settings', '2024-10-04 13:36:36.285');
INSERT INTO public.migrations VALUES (141, '/20221110144113-revert-change-request-project-options', '2024-10-04 13:36:36.288');
INSERT INTO public.migrations VALUES (142, '/20221114150559-change-request-comments', '2024-10-04 13:36:36.3');
INSERT INTO public.migrations VALUES (143, '/20221115072335-add-required-approvals', '2024-10-04 13:36:36.306');
INSERT INTO public.migrations VALUES (144, '/20221121114357-add-permission-for-environment-variants', '2024-10-04 13:36:36.31');
INSERT INTO public.migrations VALUES (145, '/20221121133546-soft-delete-user', '2024-10-04 13:36:36.312');
INSERT INTO public.migrations VALUES (146, '/20221124123914-add-favorites', '2024-10-04 13:36:36.323');
INSERT INTO public.migrations VALUES (147, '/20221125185244-change-request-unique-approvals', '2024-10-04 13:36:36.329');
INSERT INTO public.migrations VALUES (148, '/20221128165141-change-request-min-approvals', '2024-10-04 13:36:36.331');
INSERT INTO public.migrations VALUES (149, '/20221205122253-skip-change-request', '2024-10-04 13:36:36.333');
INSERT INTO public.migrations VALUES (150, '/20221220160345-user-pat-permissions', '2024-10-04 13:36:36.335');
INSERT INTO public.migrations VALUES (151, '/20221221144132-service-account-users', '2024-10-04 13:36:36.337');
INSERT INTO public.migrations VALUES (152, '/20230125065315-project-stats-table', '2024-10-04 13:36:36.345');
INSERT INTO public.migrations VALUES (153, '/20230127111638-new-project-stats-field', '2024-10-04 13:36:36.347');
INSERT INTO public.migrations VALUES (154, '/20230130113337-revert-user-pat-permissions', '2024-10-04 13:36:36.349');
INSERT INTO public.migrations VALUES (155, '/20230208084046-project-api-token-permissions', '2024-10-04 13:36:36.351');
INSERT INTO public.migrations VALUES (156, '/20230208093627-assign-project-api-token-permissions-editor', '2024-10-04 13:36:36.353');
INSERT INTO public.migrations VALUES (157, '/20230208093750-assign-project-api-token-permissions-owner', '2024-10-04 13:36:36.356');
INSERT INTO public.migrations VALUES (158, '/20230208093942-assign-project-api-token-permissions-member', '2024-10-04 13:36:36.358');
INSERT INTO public.migrations VALUES (159, '/20230222084211-add-login-events-table', '2024-10-04 13:36:36.371');
INSERT INTO public.migrations VALUES (160, '/20230222154915-create-notiications-table', '2024-10-04 13:36:36.384');
INSERT INTO public.migrations VALUES (161, '/20230224093446-drop-createdBy-from-notifications-table', '2024-10-04 13:36:36.388');
INSERT INTO public.migrations VALUES (162, '/20230227115320-rename-login-events-table-to-sign-on-log', '2024-10-04 13:36:36.39');
INSERT INTO public.migrations VALUES (163, '/20230227120500-change-display-name-for-variants-per-env-permission', '2024-10-04 13:36:36.392');
INSERT INTO public.migrations VALUES (164, '/20230227123106-add-setting-for-sign-on-log-retention', '2024-10-04 13:36:36.394');
INSERT INTO public.migrations VALUES (165, '/20230302133740-rename-sign-on-log-table-to-login-history', '2024-10-04 13:36:36.396');
INSERT INTO public.migrations VALUES (166, '/20230306103400-add-project-column-to-segments', '2024-10-04 13:36:36.399');
INSERT INTO public.migrations VALUES (167, '/20230306103400-remove-direct-link-from-segment-permissions-to-admin', '2024-10-04 13:36:36.401');
INSERT INTO public.migrations VALUES (168, '/20230309174400-add-project-segment-permission', '2024-10-04 13:36:36.404');
INSERT INTO public.migrations VALUES (169, '/20230314131041-project-settings', '2024-10-04 13:36:36.41');
INSERT INTO public.migrations VALUES (170, '/20230316092547-remove-project-stats-column', '2024-10-04 13:36:36.412');
INSERT INTO public.migrations VALUES (171, '/20230411085947-skip-change-request-ui', '2024-10-04 13:36:36.414');
INSERT INTO public.migrations VALUES (172, '/20230412062635-add-change-request-title', '2024-10-04 13:36:36.417');
INSERT INTO public.migrations VALUES (173, '/20230412125618-add-title-to-strategy', '2024-10-04 13:36:36.421');
INSERT INTO public.migrations VALUES (174, '/20230414105818-add-root-role-to-groups', '2024-10-04 13:36:36.426');
INSERT INTO public.migrations VALUES (175, '/20230419104126-add-disabled-field-to-feature-strategy', '2024-10-04 13:36:36.43');
INSERT INTO public.migrations VALUES (176, '/20230420125500-v5-strategy-changes', '2024-10-04 13:36:36.434');
INSERT INTO public.migrations VALUES (177, '/20230420211308-update-context-fields-add-sessionId', '2024-10-04 13:36:36.439');
INSERT INTO public.migrations VALUES (178, '/20230424090942-project-default-strategy-settings', '2024-10-04 13:36:36.445');
INSERT INTO public.migrations VALUES (179, '/20230504145945-variant-metrics', '2024-10-04 13:36:36.455');
INSERT INTO public.migrations VALUES (180, '/20230510113903-fix-api-token-username-migration', '2024-10-04 13:36:36.458');
INSERT INTO public.migrations VALUES (181, '/20230615122909-fix-env-sort-order', '2024-10-04 13:36:36.461');
INSERT INTO public.migrations VALUES (182, '/20230619105029-new-fine-grained-api-token-permissions', '2024-10-04 13:36:36.465');
INSERT INTO public.migrations VALUES (183, '/20230619110243-assign-apitoken-permissions-to-rootroles', '2024-10-04 13:36:36.472');
INSERT INTO public.migrations VALUES (184, '/20230621141239-refactor-api-token-permissions', '2024-10-04 13:36:36.474');
INSERT INTO public.migrations VALUES (185, '/20230630080126-delete-deprecated-permissions', '2024-10-04 13:36:36.476');
INSERT INTO public.migrations VALUES (186, '/20230706123907-events-announced-column', '2024-10-04 13:36:36.478');
INSERT INTO public.migrations VALUES (187, '/20230711094214-add-potentially-stale-flag', '2024-10-04 13:36:36.482');
INSERT INTO public.migrations VALUES (188, '/20230711163311-project-feature-limit', '2024-10-04 13:36:36.485');
INSERT INTO public.migrations VALUES (189, '/20230712091834-strategy-variants', '2024-10-04 13:36:36.489');
INSERT INTO public.migrations VALUES (190, '/20230802092725-add-last-seen-column-to-feature-environments', '2024-10-04 13:36:36.493');
INSERT INTO public.migrations VALUES (191, '/20230802141830-add-feature-and-environment-last-seen-at-to-features-view', '2024-10-04 13:36:36.497');
INSERT INTO public.migrations VALUES (192, '/20230803061359-change-request-optional-feature', '2024-10-04 13:36:36.5');
INSERT INTO public.migrations VALUES (193, '/20230808104232-update-root-roles-descriptions', '2024-10-04 13:36:36.502');
INSERT INTO public.migrations VALUES (194, '/20230814095253-change-request-rejections', '2024-10-04 13:36:36.515');
INSERT INTO public.migrations VALUES (195, '/20230814115436-change-request-timzone-timestamps', '2024-10-04 13:36:36.584');
INSERT INTO public.migrations VALUES (196, '/20230815065908-change-request-approve-reject-permission', '2024-10-04 13:36:36.618');
INSERT INTO public.migrations VALUES (197, '/20230817095805-client-applications-usage-table', '2024-10-04 13:36:36.631');
INSERT INTO public.migrations VALUES (198, '/20230818124614-update-client-applications-usage-table', '2024-10-04 13:36:36.644');
INSERT INTO public.migrations VALUES (199, '/20230830121352-update-client-applications-usage-table', '2024-10-04 13:36:36.653');
INSERT INTO public.migrations VALUES (200, '/20230905122605-add-feature-naming-description', '2024-10-04 13:36:36.66');
INSERT INTO public.migrations VALUES (201, '/20230919104006-dependent-features', '2024-10-04 13:36:36.662');
INSERT INTO public.migrations VALUES (202, '/20230927071830-reset-pnps-feedback', '2024-10-04 13:36:36.674');
INSERT INTO public.migrations VALUES (203, '/20230927172930-events-announced-index', '2024-10-04 13:36:36.677');
INSERT INTO public.migrations VALUES (204, '/20231002122426-update-dependency-permission', '2024-10-04 13:36:36.685');
INSERT INTO public.migrations VALUES (205, '/20231003113443-last-seen-at-metrics-table', '2024-10-04 13:36:36.687');
INSERT INTO public.migrations VALUES (206, '/20231004120900-create-changes-stats-table-and-trigger', '2024-10-04 13:36:36.708');
INSERT INTO public.migrations VALUES (207, '/20231012082537-message-banners', '2024-10-04 13:36:36.721');
INSERT INTO public.migrations VALUES (208, '/20231019110154-rename-message-banners-table-to-banners', '2024-10-04 13:36:36.725');
INSERT INTO public.migrations VALUES (209, '/20231024121307-add-change-request-schedule', '2024-10-04 13:36:36.733');
INSERT INTO public.migrations VALUES (210, '/20231025093422-default-project-mode', '2024-10-04 13:36:36.739');
INSERT INTO public.migrations VALUES (211, '/20231030091931-add-created-by-and-status-change-request-schedule', '2024-10-04 13:36:36.747');
INSERT INTO public.migrations VALUES (212, '/20231103064746-change-request-schedule-change-type', '2024-10-04 13:36:36.774');
INSERT INTO public.migrations VALUES (213, '/20231121153304-add-permission-create-tag-type', '2024-10-04 13:36:36.777');
INSERT INTO public.migrations VALUES (214, '/20231122121456-dedupe-any-duplicate-permissions', '2024-10-04 13:36:36.781');
INSERT INTO public.migrations VALUES (215, '/20231123100052-drop-last-seen-foreign-key', '2024-10-04 13:36:36.784');
INSERT INTO public.migrations VALUES (216, '/20231123155649-favor-permission-name-over-id', '2024-10-04 13:36:36.814');
INSERT INTO public.migrations VALUES (217, '/20231211121444-features-created-by', '2024-10-04 13:36:36.821');
INSERT INTO public.migrations VALUES (218, '/20231211122322-feature-types-created-by', '2024-10-04 13:36:36.823');
INSERT INTO public.migrations VALUES (219, '/20231211122351-feature-tag-created-by', '2024-10-04 13:36:36.825');
INSERT INTO public.migrations VALUES (220, '/20231211122426-feature-strategies-created-by', '2024-10-04 13:36:36.828');
INSERT INTO public.migrations VALUES (221, '/20231211132341-add-created-by-to-role-permission', '2024-10-04 13:36:36.83');
INSERT INTO public.migrations VALUES (222, '/20231211133008-add-created-by-to-role-user', '2024-10-04 13:36:36.832');
INSERT INTO public.migrations VALUES (223, '/20231211133920-add-created-by-to-roles', '2024-10-04 13:36:36.835');
INSERT INTO public.migrations VALUES (224, '/20231211134130-add-created-by-to-users', '2024-10-04 13:36:36.837');
INSERT INTO public.migrations VALUES (225, '/20231211134633-add-created-by-to-apitokens', '2024-10-04 13:36:36.839');
INSERT INTO public.migrations VALUES (226, '/20231212094044-event-created-by-user-id', '2024-10-04 13:36:36.844');
INSERT INTO public.migrations VALUES (227, '/20231213111906-add-reason-to-change-request-schedule', '2024-10-04 13:36:36.846');
INSERT INTO public.migrations VALUES (228, '/20231215105713-incoming-webhooks', '2024-10-04 13:36:36.876');
INSERT INTO public.migrations VALUES (229, '/20231218165612-inc-webhook-tokens-rename-secret-to-token', '2024-10-04 13:36:36.879');
INSERT INTO public.migrations VALUES (230, '/20231219100343-rename-new-columns-to-created-by-user-id', '2024-10-04 13:36:36.883');
INSERT INTO public.migrations VALUES (231, '/20231221143955-feedback-table', '2024-10-04 13:36:36.895');
INSERT INTO public.migrations VALUES (232, '/20231222071533-unleash-system-user', '2024-10-04 13:36:36.901');
INSERT INTO public.migrations VALUES (233, '/20240102142100-incoming-webhooks-created-by', '2024-10-04 13:36:36.905');
INSERT INTO public.migrations VALUES (234, '/20240102205517-observable-events', '2024-10-04 13:36:36.926');
INSERT INTO public.migrations VALUES (235, '/20240108151652-add-daily-metrics', '2024-10-04 13:36:36.948');
INSERT INTO public.migrations VALUES (236, '/20240109093021-incoming-webhooks-description', '2024-10-04 13:36:36.951');
INSERT INTO public.migrations VALUES (237, '/20240109095348-add-reason-column-to-schedule', '2024-10-04 13:36:36.954');
INSERT INTO public.migrations VALUES (238, '/20240111075911-update-system-user-email', '2024-10-04 13:36:36.957');
INSERT INTO public.migrations VALUES (239, '/20240111125100-automated-actions', '2024-10-04 13:36:36.984');
INSERT INTO public.migrations VALUES (240, '/20240116104456-drop-unused-column-permissionid', '2024-10-04 13:36:36.987');
INSERT INTO public.migrations VALUES (241, '/20240116154700-unleash-admin-token-user', '2024-10-04 13:36:36.99');
INSERT INTO public.migrations VALUES (242, '/20240117093601-add-more-granular-project-permissions', '2024-10-04 13:36:36.995');
INSERT INTO public.migrations VALUES (243, '/20240118093611-missing-primary-keys', '2024-10-04 13:36:37.025');
INSERT INTO public.migrations VALUES (244, '/20240119171200-action-states', '2024-10-04 13:36:37.049');
INSERT INTO public.migrations VALUES (245, '/20240124123000-add-enabled-to-action-sets', '2024-10-04 13:36:37.052');
INSERT INTO public.migrations VALUES (246, '/20240125084701-add-user-trends', '2024-10-04 13:36:37.059');
INSERT INTO public.migrations VALUES (247, '/20240125085703-users-table-increae-image-url-size', '2024-10-04 13:36:37.062');
INSERT INTO public.migrations VALUES (248, '/20240125090553-events-fix-incorrectly-assigned-sysuser-id', '2024-10-04 13:36:37.066');
INSERT INTO public.migrations VALUES (249, '/20240125100000-events-system-user-old2new', '2024-10-04 13:36:37.068');
INSERT INTO public.migrations VALUES (250, '/20240126095544-add-flag-trends', '2024-10-04 13:36:37.079');
INSERT INTO public.migrations VALUES (251, '/20240130104757-flag-trends-health-time-to-production', '2024-10-04 13:36:37.082');
INSERT INTO public.migrations VALUES (252, '/20240207164033-client-applications-announced-index', '2024-10-04 13:36:37.087');
INSERT INTO public.migrations VALUES (253, '/20240208123212-create-stat-traffic-usage-table', '2024-10-04 13:36:37.108');
INSERT INTO public.migrations VALUES (254, '/20240208130439-events-revision-id-index', '2024-10-04 13:36:37.114');
INSERT INTO public.migrations VALUES (255, '/20240215133213-flag-trends-users', '2024-10-04 13:36:37.117');
INSERT INTO public.migrations VALUES (256, '/20240220130622-add-action-state-indexes', '2024-10-04 13:36:37.128');
INSERT INTO public.migrations VALUES (257, '/20240221082758-action-events', '2024-10-04 13:36:37.147');
INSERT INTO public.migrations VALUES (258, '/20240221115502-drop-action-states', '2024-10-04 13:36:37.151');
INSERT INTO public.migrations VALUES (259, '/20240222123532-project-metrics-summary-trends', '2024-10-04 13:36:37.174');
INSERT INTO public.migrations VALUES (260, '/20240229093231-drop-fk-and-cascade-in-trends', '2024-10-04 13:36:37.177');
INSERT INTO public.migrations VALUES (261, '/20240304084102-rename-observable-events-to-signals', '2024-10-04 13:36:37.198');
INSERT INTO public.migrations VALUES (262, '/20240304160659-add-environment-type-trends', '2024-10-04 13:36:37.226');
INSERT INTO public.migrations VALUES (263, '/20240305094305-features-remove-archived', '2024-10-04 13:36:37.231');
INSERT INTO public.migrations VALUES (264, '/20240305121426-add-created-at-environment-type-trends', '2024-10-04 13:36:37.235');
INSERT INTO public.migrations VALUES (265, '/20240305121702-add-metrics-summary-columns-to-flag-trends', '2024-10-04 13:36:37.238');
INSERT INTO public.migrations VALUES (266, '/20240305131822-add-scim-id-column-to-user', '2024-10-04 13:36:37.245');
INSERT INTO public.migrations VALUES (267, '/20240306145609-make-scim-id-idx-unique', '2024-10-04 13:36:37.252');
INSERT INTO public.migrations VALUES (268, '/20240325081847-add-scim-id-for-groups', '2024-10-04 13:36:37.261');
INSERT INTO public.migrations VALUES (269, '/20240326122126-add-index-on-group-name', '2024-10-04 13:36:37.267');
INSERT INTO public.migrations VALUES (270, '/20240329064629-revert-feature-archived', '2024-10-04 13:36:37.271');
INSERT INTO public.migrations VALUES (271, '/20240405120422-add-feature-lifecycles', '2024-10-04 13:36:37.285');
INSERT INTO public.migrations VALUES (272, '/20240405174629-jobs', '2024-10-04 13:36:37.304');
INSERT INTO public.migrations VALUES (273, '/20240408104624-fix-environment-type-trends', '2024-10-04 13:36:37.322');
INSERT INTO public.migrations VALUES (274, '/20240418140646-add-ip-column-to-events-table', '2024-10-04 13:36:37.336');
INSERT INTO public.migrations VALUES (275, '/20240425132155-flag-trends-bigint', '2024-10-04 13:36:37.361');
INSERT INTO public.migrations VALUES (276, '/20240430075605-add-scim-external-id', '2024-10-04 13:36:37.384');
INSERT INTO public.migrations VALUES (277, '/20240506141345-lifecycle-initial-stage', '2024-10-04 13:36:37.389');
INSERT INTO public.migrations VALUES (278, '/20240507075431-client-metrics-env-daily-bigint', '2024-10-04 13:36:37.414');
INSERT INTO public.migrations VALUES (279, '/20240508153244-feature-lifecycles-status', '2024-10-04 13:36:37.43');
INSERT INTO public.migrations VALUES (280, '/20240523093355-toggle-to-flag-rename', '2024-10-04 13:36:37.433');
INSERT INTO public.migrations VALUES (281, '/20240523113322-roles-toggle-to-flag-rename', '2024-10-04 13:36:37.436');
INSERT INTO public.migrations VALUES (282, '/20240611092538-add-created-by-to-features-view', '2024-10-04 13:36:37.44');
INSERT INTO public.migrations VALUES (283, '/20240705111827-used-passwords-table', '2024-10-04 13:36:37.463');
INSERT INTO public.migrations VALUES (284, '/20240716135038-integration-events', '2024-10-04 13:36:37.48');
INSERT INTO public.migrations VALUES (285, '/20240806140453-add-archived-at-to-projects', '2024-10-04 13:36:37.483');
INSERT INTO public.migrations VALUES (286, '/20240812120954-add-archived-at-to-projects', '2024-10-04 13:36:37.49');
INSERT INTO public.migrations VALUES (287, '/20240812132633-events-type-index', '2024-10-04 13:36:37.495');
INSERT INTO public.migrations VALUES (288, '/20240821141555-segment-no-project-cleanup', '2024-10-04 13:36:37.502');
INSERT INTO public.migrations VALUES (289, '/20240823091442-normalize-token-types', '2024-10-04 13:36:37.505');
INSERT INTO public.migrations VALUES (290, '/20240828154255-user-first-seen-at', '2024-10-04 13:36:37.507');
INSERT INTO public.migrations VALUES (291, '/20240830102144-onboarding-events', '2024-10-04 13:36:37.524');
INSERT INTO public.migrations VALUES (292, '/20240903152133-clear-onboarding-events', '2024-10-04 13:36:37.529');
INSERT INTO public.migrations VALUES (293, '/20240904084114-add-update-feature-dependency-editor', '2024-10-04 13:36:37.534');
INSERT INTO public.migrations VALUES (294, '/20240919083625-client-metrics-env-variants-daily-to-bigint', '2024-10-04 13:36:37.549');


--
-- Data for Name: notifications; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: onboarding_events_instance; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: onboarding_events_project; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: permissions; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.permissions VALUES (1, 'ADMIN', 'Admin', 'root', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (3, 'CREATE_STRATEGY', 'Create activation strategies', 'root', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (4, 'CREATE_ADDON', 'Create addons', 'root', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (5, 'DELETE_ADDON', 'Delete addons', 'root', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (6, 'UPDATE_ADDON', 'Update addons', 'root', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (9, 'UPDATE_APPLICATION', 'Update applications', 'root', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (10, 'UPDATE_TAG_TYPE', 'Update tag types', 'root', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (11, 'DELETE_TAG_TYPE', 'Delete tag types', 'root', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (12, 'CREATE_PROJECT', 'Create projects', 'root', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (13, 'UPDATE_PROJECT', 'Update project', 'project', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (14, 'DELETE_PROJECT', 'Delete project', 'project', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (15, 'UPDATE_STRATEGY', 'Update strategies', 'root', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (16, 'DELETE_STRATEGY', 'Delete strategies', 'root', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (17, 'UPDATE_CONTEXT_FIELD', 'Update context fields', 'root', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (18, 'CREATE_CONTEXT_FIELD', 'Create context fields', 'root', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (19, 'DELETE_CONTEXT_FIELD', 'Delete context fields', 'root', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (20, 'READ_ROLE', 'Read roles', 'root', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (25, 'CREATE_FEATURE_STRATEGY', 'Create activation strategies', 'environment', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (26, 'UPDATE_FEATURE_STRATEGY', 'Update activation strategies', 'environment', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (27, 'DELETE_FEATURE_STRATEGY', 'Delete activation strategies', 'environment', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (50, 'CREATE_CLIENT_API_TOKEN', 'Create CLIENT API tokens', 'root', '2024-10-04 13:36:36.463389+13');
INSERT INTO public.permissions VALUES (29, 'UPDATE_FEATURE_VARIANTS', 'Create/edit variants', 'project', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (31, 'CREATE_SEGMENT', 'Create segments', 'root', '2024-10-04 13:36:36.020626+13');
INSERT INTO public.permissions VALUES (32, 'UPDATE_SEGMENT', 'Edit segments', 'root', '2024-10-04 13:36:36.020626+13');
INSERT INTO public.permissions VALUES (33, 'DELETE_SEGMENT', 'Delete segments', 'root', '2024-10-04 13:36:36.020626+13');
INSERT INTO public.permissions VALUES (42, 'READ_PROJECT_API_TOKEN', 'Read api tokens for a specific project', 'project', '2024-10-04 13:36:36.351363+13');
INSERT INTO public.permissions VALUES (43, 'CREATE_PROJECT_API_TOKEN', 'Create api tokens for a specific project', 'project', '2024-10-04 13:36:36.351363+13');
INSERT INTO public.permissions VALUES (44, 'DELETE_PROJECT_API_TOKEN', 'Delete api tokens for a specific project', 'project', '2024-10-04 13:36:36.351363+13');
INSERT INTO public.permissions VALUES (37, 'UPDATE_FEATURE_ENVIRONMENT_VARIANTS', 'Update variants', 'environment', '2024-10-04 13:36:36.308117+13');
INSERT INTO public.permissions VALUES (36, 'APPLY_CHANGE_REQUEST', 'Apply change requests', 'environment', '2024-10-04 13:36:36.278016+13');
INSERT INTO public.permissions VALUES (51, 'UPDATE_CLIENT_API_TOKEN', 'Update CLIENT API tokens', 'root', '2024-10-04 13:36:36.463389+13');
INSERT INTO public.permissions VALUES (45, 'UPDATE_PROJECT_SEGMENT', 'Create/edit project segment', 'project', '2024-10-04 13:36:36.403506+13');
INSERT INTO public.permissions VALUES (38, 'SKIP_CHANGE_REQUEST', 'Skip change request process', 'environment', '2024-10-04 13:36:36.333003+13');
INSERT INTO public.permissions VALUES (52, 'DELETE_CLIENT_API_TOKEN', 'Delete CLIENT API tokens', 'root', '2024-10-04 13:36:36.463389+13');
INSERT INTO public.permissions VALUES (53, 'READ_CLIENT_API_TOKEN', 'Read CLIENT API tokens', 'root', '2024-10-04 13:36:36.463389+13');
INSERT INTO public.permissions VALUES (35, 'APPROVE_CHANGE_REQUEST', 'Approve/Reject change requests', 'environment', '2024-10-04 13:36:36.278016+13');
INSERT INTO public.permissions VALUES (2, 'CREATE_FEATURE', 'Create feature flags', 'project', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (7, 'UPDATE_FEATURE', 'Update feature flags', 'project', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (8, 'DELETE_FEATURE', 'Delete feature flags', 'project', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (30, 'MOVE_FEATURE_TOGGLE', 'Change feature flag project', 'project', '2024-10-04 13:36:35.942565+13');
INSERT INTO public.permissions VALUES (28, 'UPDATE_FEATURE_ENVIRONMENT', 'Enable/disable flags', 'environment', '2024-10-04 13:36:35.857877+13');
INSERT INTO public.permissions VALUES (54, 'CREATE_FRONTEND_API_TOKEN', 'Create FRONTEND API tokens', 'root', '2024-10-04 13:36:36.463389+13');
INSERT INTO public.permissions VALUES (55, 'UPDATE_FRONTEND_API_TOKEN', 'Update FRONTEND API tokens', 'root', '2024-10-04 13:36:36.463389+13');
INSERT INTO public.permissions VALUES (56, 'DELETE_FRONTEND_API_TOKEN', 'Delete FRONTEND API tokens', 'root', '2024-10-04 13:36:36.463389+13');
INSERT INTO public.permissions VALUES (57, 'READ_FRONTEND_API_TOKEN', 'Read FRONTEND API tokens', 'root', '2024-10-04 13:36:36.463389+13');
INSERT INTO public.permissions VALUES (58, 'UPDATE_FEATURE_DEPENDENCY', 'Update feature dependency', 'project', '2024-10-04 13:36:36.684812+13');
INSERT INTO public.permissions VALUES (59, 'CREATE_TAG_TYPE', 'Create tag types', 'root', '2024-10-04 13:36:36.776393+13');
INSERT INTO public.permissions VALUES (60, 'PROJECT_USER_ACCESS_READ', 'View only access to Project User Access', 'project', '2024-10-04 13:36:36.99232+13');
INSERT INTO public.permissions VALUES (61, 'PROJECT_DEFAULT_STRATEGY_READ', 'View only access to default strategy configuration for project', 'project', '2024-10-04 13:36:36.99232+13');
INSERT INTO public.permissions VALUES (62, 'PROJECT_CHANGE_REQUEST_READ', 'View only access to change request configuration for project', 'project', '2024-10-04 13:36:36.99232+13');
INSERT INTO public.permissions VALUES (63, 'PROJECT_SETTINGS_READ', 'View only access to project settings', 'project', '2024-10-04 13:36:36.99232+13');
INSERT INTO public.permissions VALUES (64, 'PROJECT_USER_ACCESS_WRITE', 'Write access to Project User Access', 'project', '2024-10-04 13:36:36.99232+13');
INSERT INTO public.permissions VALUES (65, 'PROJECT_DEFAULT_STRATEGY_WRITE', 'Write access to default strategy configuration for project', 'project', '2024-10-04 13:36:36.99232+13');
INSERT INTO public.permissions VALUES (66, 'PROJECT_CHANGE_REQUEST_WRITE', 'Write access to change request configuration for project', 'project', '2024-10-04 13:36:36.99232+13');
INSERT INTO public.permissions VALUES (67, 'PROJECT_SETTINGS_WRITE', 'Write access to project settings', 'project', '2024-10-04 13:36:36.99232+13');


--
-- Data for Name: personal_access_tokens; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: project_client_metrics_trends; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: project_environments; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.project_environments VALUES ('default', 'development', NULL);
INSERT INTO public.project_environments VALUES ('default', 'production', NULL);


--
-- Data for Name: project_settings; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: project_stats; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.project_stats VALUES ('default', 0, 19, 0, 4, 0, 0, 0, 0);


--
-- Data for Name: projects; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.projects VALUES ('default', 'Default', 'Default project', '2024-10-04 13:36:35.352718', 100, '2024-10-04 13:41:52.817+13', NULL);


--
-- Data for Name: public_signup_tokens; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: public_signup_tokens_user; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: reset_tokens; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: role_permission; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.931056+13', 'development', 'CREATE_FEATURE_STRATEGY', NULL, 1);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.931056+13', 'development', 'UPDATE_FEATURE_STRATEGY', NULL, 2);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.931056+13', 'development', 'DELETE_FEATURE_STRATEGY', NULL, 3);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.931056+13', 'development', 'UPDATE_FEATURE_ENVIRONMENT', NULL, 4);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.931056+13', 'production', 'CREATE_FEATURE_STRATEGY', NULL, 5);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.931056+13', 'production', 'UPDATE_FEATURE_STRATEGY', NULL, 6);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.931056+13', 'production', 'DELETE_FEATURE_STRATEGY', NULL, 7);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.931056+13', 'production', 'UPDATE_FEATURE_ENVIRONMENT', NULL, 8);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.931056+13', 'default', 'CREATE_FEATURE_STRATEGY', NULL, 9);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.931056+13', 'default', 'UPDATE_FEATURE_STRATEGY', NULL, 10);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.931056+13', 'default', 'DELETE_FEATURE_STRATEGY', NULL, 11);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.931056+13', 'default', 'UPDATE_FEATURE_ENVIRONMENT', NULL, 12);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:35.931056+13', 'development', 'CREATE_FEATURE_STRATEGY', NULL, 13);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:35.931056+13', 'development', 'UPDATE_FEATURE_STRATEGY', NULL, 14);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:35.931056+13', 'development', 'DELETE_FEATURE_STRATEGY', NULL, 15);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:35.931056+13', 'development', 'UPDATE_FEATURE_ENVIRONMENT', NULL, 16);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:35.931056+13', 'production', 'CREATE_FEATURE_STRATEGY', NULL, 17);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:35.931056+13', 'production', 'UPDATE_FEATURE_STRATEGY', NULL, 18);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:35.931056+13', 'production', 'DELETE_FEATURE_STRATEGY', NULL, 19);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:35.931056+13', 'production', 'UPDATE_FEATURE_ENVIRONMENT', NULL, 20);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:35.931056+13', 'default', 'CREATE_FEATURE_STRATEGY', NULL, 21);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:35.931056+13', 'default', 'UPDATE_FEATURE_STRATEGY', NULL, 22);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:35.931056+13', 'default', 'DELETE_FEATURE_STRATEGY', NULL, 23);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:35.931056+13', 'default', 'UPDATE_FEATURE_ENVIRONMENT', NULL, 24);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.937366+13', 'development', 'CREATE_FEATURE_STRATEGY', NULL, 25);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.937366+13', 'development', 'UPDATE_FEATURE_STRATEGY', NULL, 26);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.937366+13', 'development', 'DELETE_FEATURE_STRATEGY', NULL, 27);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.937366+13', 'development', 'UPDATE_FEATURE_ENVIRONMENT', NULL, 28);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.937366+13', 'production', 'CREATE_FEATURE_STRATEGY', NULL, 29);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.937366+13', 'production', 'UPDATE_FEATURE_STRATEGY', NULL, 30);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.937366+13', 'production', 'DELETE_FEATURE_STRATEGY', NULL, 31);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.937366+13', 'production', 'UPDATE_FEATURE_ENVIRONMENT', NULL, 32);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.937366+13', 'default', 'CREATE_FEATURE_STRATEGY', NULL, 33);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.937366+13', 'default', 'UPDATE_FEATURE_STRATEGY', NULL, 34);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.937366+13', 'default', 'DELETE_FEATURE_STRATEGY', NULL, 35);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.937366+13', 'default', 'UPDATE_FEATURE_ENVIRONMENT', NULL, 36);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'CREATE_FEATURE', NULL, 37);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'CREATE_STRATEGY', NULL, 38);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'CREATE_ADDON', NULL, 39);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'DELETE_ADDON', NULL, 40);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'UPDATE_ADDON', NULL, 41);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'UPDATE_FEATURE', NULL, 42);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'DELETE_FEATURE', NULL, 43);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'UPDATE_APPLICATION', NULL, 44);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'UPDATE_TAG_TYPE', NULL, 45);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'DELETE_TAG_TYPE', NULL, 46);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'CREATE_PROJECT', NULL, 47);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'UPDATE_PROJECT', NULL, 48);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'DELETE_PROJECT', NULL, 49);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'UPDATE_STRATEGY', NULL, 50);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'DELETE_STRATEGY', NULL, 51);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'UPDATE_CONTEXT_FIELD', NULL, 52);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'CREATE_CONTEXT_FIELD', NULL, 53);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'DELETE_CONTEXT_FIELD', NULL, 54);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.857877+13', '', 'UPDATE_FEATURE_VARIANTS', NULL, 55);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.857877+13', '', 'CREATE_FEATURE', NULL, 56);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.857877+13', '', 'UPDATE_FEATURE', NULL, 57);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.857877+13', '', 'DELETE_FEATURE', NULL, 58);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.857877+13', '', 'UPDATE_PROJECT', NULL, 59);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.857877+13', '', 'DELETE_PROJECT', NULL, 60);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.857877+13', '', 'UPDATE_FEATURE_VARIANTS', NULL, 61);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:35.857877+13', '', 'CREATE_FEATURE', NULL, 62);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:35.857877+13', '', 'UPDATE_FEATURE', NULL, 63);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:35.857877+13', '', 'DELETE_FEATURE', NULL, 64);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:35.857877+13', '', 'UPDATE_FEATURE_VARIANTS', NULL, 65);
INSERT INTO public.role_permission VALUES (1, '2024-10-04 13:36:35.857877+13', '', 'ADMIN', NULL, 66);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:35.942565+13', '', 'MOVE_FEATURE_TOGGLE', NULL, 67);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:35.942565+13', '', 'MOVE_FEATURE_TOGGLE', NULL, 68);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:36.020626+13', NULL, 'CREATE_SEGMENT', NULL, 69);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:36.020626+13', NULL, 'UPDATE_SEGMENT', NULL, 70);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:36.020626+13', NULL, 'DELETE_SEGMENT', NULL, 71);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:36.308117+13', 'development', 'UPDATE_FEATURE_ENVIRONMENT_VARIANTS', NULL, 72);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:36.308117+13', 'production', 'UPDATE_FEATURE_ENVIRONMENT_VARIANTS', NULL, 73);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:36.308117+13', 'default', 'UPDATE_FEATURE_ENVIRONMENT_VARIANTS', NULL, 74);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:36.308117+13', 'development', 'UPDATE_FEATURE_ENVIRONMENT_VARIANTS', NULL, 75);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:36.308117+13', 'production', 'UPDATE_FEATURE_ENVIRONMENT_VARIANTS', NULL, 76);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:36.308117+13', 'default', 'UPDATE_FEATURE_ENVIRONMENT_VARIANTS', NULL, 77);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:36.308117+13', 'development', 'UPDATE_FEATURE_ENVIRONMENT_VARIANTS', NULL, 78);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:36.308117+13', 'production', 'UPDATE_FEATURE_ENVIRONMENT_VARIANTS', NULL, 79);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:36.308117+13', 'default', 'UPDATE_FEATURE_ENVIRONMENT_VARIANTS', NULL, 80);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:36.353366+13', NULL, 'READ_PROJECT_API_TOKEN', NULL, 81);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:36.353366+13', NULL, 'CREATE_PROJECT_API_TOKEN', NULL, 82);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:36.353366+13', NULL, 'DELETE_PROJECT_API_TOKEN', NULL, 83);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:36.355429+13', NULL, 'READ_PROJECT_API_TOKEN', NULL, 84);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:36.355429+13', NULL, 'CREATE_PROJECT_API_TOKEN', NULL, 85);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:36.355429+13', NULL, 'DELETE_PROJECT_API_TOKEN', NULL, 86);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:36.357681+13', NULL, 'READ_PROJECT_API_TOKEN', NULL, 87);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:36.357681+13', NULL, 'CREATE_PROJECT_API_TOKEN', NULL, 88);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:36.357681+13', NULL, 'DELETE_PROJECT_API_TOKEN', NULL, 89);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:36.467313+13', NULL, 'READ_CLIENT_API_TOKEN', NULL, 90);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:36.467313+13', NULL, 'READ_FRONTEND_API_TOKEN', NULL, 91);
INSERT INTO public.role_permission VALUES (5, '2024-10-04 13:36:36.684812+13', NULL, 'UPDATE_FEATURE_DEPENDENCY', NULL, 92);
INSERT INTO public.role_permission VALUES (4, '2024-10-04 13:36:36.684812+13', NULL, 'UPDATE_FEATURE_DEPENDENCY', NULL, 93);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:36.776393+13', NULL, 'CREATE_TAG_TYPE', NULL, 94);
INSERT INTO public.role_permission VALUES (2, '2024-10-04 13:36:37.531488+13', NULL, 'UPDATE_FEATURE_DEPENDENCY', NULL, 95);


--
-- Data for Name: role_user; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.role_user VALUES (1, 1, '2024-10-04 13:36:37.896108+13', 'default', NULL);


--
-- Data for Name: roles; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.roles VALUES (1, 'Admin', 'Users with the root admin role have superuser access to Unleash and can perform any operation within the Unleash platform.', 'root', '2024-10-04 13:36:35.453463+13', NULL, NULL);
INSERT INTO public.roles VALUES (2, 'Editor', 'Users with the root editor role have access to most features in Unleash, but can not manage users and roles in the root scope. Editors will be added as project owners when creating projects and get superuser rights within the context of these projects. Users with the editor role will also get access to most permissions on the default project by default.', 'root', '2024-10-04 13:36:35.453463+13', NULL, NULL);
INSERT INTO public.roles VALUES (3, 'Viewer', 'Users with the root viewer role can only read root resources in Unleash. Viewers can be added to specific projects as project members. Users with the viewer role may not view API tokens.', 'root', '2024-10-04 13:36:35.453463+13', NULL, NULL);
INSERT INTO public.roles VALUES (4, 'Owner', 'Users with the project owner role have full control over the project, and can add and manage other users within the project context, manage feature flags within the project, and control advanced project features like archiving and deleting the project.', 'project', '2024-10-04 13:36:35.555596+13', NULL, NULL);
INSERT INTO public.roles VALUES (5, 'Member', 'Users with the project member role are allowed to view, create, and update feature flags within a project, but have limited permissions in regards to managing the project''s user access and can not archive or delete the project.', 'project', '2024-10-04 13:36:35.555596+13', NULL, NULL);


--
-- Data for Name: segments; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: settings; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.settings VALUES ('unleash.secret', '"784975a332988c6ea59163ad2b0adc2a2cfa6e45"');
INSERT INTO public.settings VALUES ('instanceInfo', '{"id" : "cf1eee91-a854-41cf-905b-0173fd6550af"}');
INSERT INTO public.settings VALUES ('login_history_retention', '{"hours": 336}');


--
-- Data for Name: signal_endpoint_tokens; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: signal_endpoints; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: signals; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: stat_environment_updates; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.stat_environment_updates VALUES ('2024-10-04', 'development', 6);
INSERT INTO public.stat_environment_updates VALUES ('2024-10-04', 'production', 9);


--
-- Data for Name: stat_traffic_usage; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: strategies; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.strategies VALUES ('2024-10-04 13:36:35.087127+13', 'remoteAddress', 'Enable the feature for a specific set of IP addresses.', '[{"name":"IPs","type":"list","description":"List of IPs to enable the feature toggle for.","required":true}]', 1, false, 3, 'IPs', NULL);
INSERT INTO public.strategies VALUES ('2024-10-04 13:36:35.087127+13', 'applicationHostname', 'Enable the feature for a specific set of hostnames.', '[{"name":"hostNames","type":"list","description":"List of hostnames to enable the feature toggle for.","required":false}]', 1, false, 4, 'Hosts', NULL);
INSERT INTO public.strategies VALUES ('2024-10-04 13:36:35.001622+13', 'default', 'This strategy turns on / off for your entire userbase. Prefer using "Gradual rollout" strategy (100%=on, 0%=off).', '[]', 1, false, 1, 'Standard', NULL);
INSERT INTO public.strategies VALUES ('2024-10-04 13:36:35.266242+13', 'flexibleRollout', 'Roll out to a percentage of your userbase, and ensure that the experience is the same for the user on each visit.', '[{"name":"rollout","type":"percentage","description":"","required":false},{"name":"stickiness","type":"string","description":"Used define stickiness. Possible values: default, userId, sessionId, random","required":true},{"name":"groupId","type":"string","description":"Used to define a activation groups, which allows you to correlate across feature toggles.","required":true}]', 1, false, 0, 'Gradual rollout', NULL);
INSERT INTO public.strategies VALUES ('2024-10-04 13:36:35.087127+13', 'userWithId', 'Enable the feature for a specific set of userIds. Prefer using "Gradual rollout" strategy with user id constraints.', '[{"name":"userIds","type":"list","description":"","required":false}]', 1, true, 2, 'UserIDs', NULL);


--
-- Data for Name: tag_types; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.tag_types VALUES ('simple', 'Used to simplify filtering of features', '#', '2024-10-04 13:36:35.374181+13');


--
-- Data for Name: tags; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: unleash_session; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.unleash_session VALUES ('6ib_dM73jxOQxhY9ehBTGvHW6iuKfxEY', '{"cookie":{"originalMaxAge":172800000,"expires":"2024-10-06T00:36:43.981Z","secure":false,"httpOnly":true,"path":"/","sameSite":"lax"},"user":{"isAPI":false,"accountType":"User","id":1,"username":"admin","imageUrl":"https://gravatar.com/avatar/8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918?s=42&d=retro&r=g","seenAt":null,"loginAttempts":0,"createdAt":"2024-10-04T00:36:37.748Z","scimId":null}}', '2024-10-04 13:36:43.983121+13', '2024-10-06 13:45:50.065+13');


--
-- Data for Name: used_passwords; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.used_passwords VALUES (1, '$2a$10$g6xOe/AGG1WOTqkqHp6XH.gjVJEPEGvVTxkPci2h7g79OUnkjGyne', '2024-10-04 00:36:37.887097+13');


--
-- Data for Name: user_feedback; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: user_notifications; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: user_splash; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: user_trends; Type: TABLE DATA; Schema: public; Owner: root
--



--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: root
--

INSERT INTO public.users VALUES (-1337, 'Unleash System', 'unleash_system_user', NULL, NULL, NULL, 0, '2024-10-04 13:36:36.897716', NULL, NULL, '[]', NULL, false, -1337, true, NULL, NULL, NULL);
INSERT INTO public.users VALUES (-42, 'Unleash Admin Token User', 'unleash_admin_token', NULL, NULL, NULL, 0, '2024-10-04 13:36:36.989773', NULL, NULL, '[]', NULL, false, -1337, true, NULL, NULL, NULL);
INSERT INTO public.users VALUES (1, NULL, 'admin', NULL, NULL, '$2a$10$g6xOe/AGG1WOTqkqHp6XH.gjVJEPEGvVTxkPci2h7g79OUnkjGyne', 0, '2024-10-04 13:36:37.748', '2024-10-04 13:36:43.97', NULL, '[]', NULL, false, NULL, false, NULL, NULL, NULL);


--
-- Name: action_set_events_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.action_set_events_id_seq', 1, false);


--
-- Name: action_sets_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.action_sets_id_seq', 1, false);


--
-- Name: actions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.actions_id_seq', 1, false);


--
-- Name: addons_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.addons_id_seq', 1, false);


--
-- Name: change_request_approvals_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.change_request_approvals_id_seq', 1, false);


--
-- Name: change_request_comments_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.change_request_comments_id_seq', 1, false);


--
-- Name: change_request_events_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.change_request_events_id_seq', 1, false);


--
-- Name: change_request_rejections_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.change_request_rejections_id_seq', 1, false);


--
-- Name: change_requests_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.change_requests_id_seq', 1, false);


--
-- Name: events_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.events_id_seq', 25, true);


--
-- Name: feedback_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.feedback_id_seq', 1, false);


--
-- Name: groups_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.groups_id_seq', 1, false);


--
-- Name: incoming_webhook_tokens_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.incoming_webhook_tokens_id_seq', 1, false);


--
-- Name: incoming_webhooks_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.incoming_webhooks_id_seq', 1, false);


--
-- Name: integration_events_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.integration_events_id_seq', 1, false);


--
-- Name: login_events_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.login_events_id_seq', 1, false);


--
-- Name: message_banners_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.message_banners_id_seq', 1, false);


--
-- Name: migrations_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.migrations_id_seq', 294, true);


--
-- Name: notifications_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.notifications_id_seq', 1, false);


--
-- Name: observable_events_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.observable_events_id_seq', 1, false);


--
-- Name: permissions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.permissions_id_seq', 67, true);


--
-- Name: personal_access_tokens_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.personal_access_tokens_id_seq', 1, false);


--
-- Name: role_permission_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.role_permission_id_seq', 95, true);


--
-- Name: roles_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.roles_id_seq', 5, true);


--
-- Name: segments_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.segments_id_seq', 1, false);


--
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: root
--

SELECT pg_catalog.setval('public.users_id_seq', 1, true);


--
-- Name: action_set_events action_set_events_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.action_set_events
    ADD CONSTRAINT action_set_events_pkey PRIMARY KEY (id);


--
-- Name: action_sets action_sets_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.action_sets
    ADD CONSTRAINT action_sets_pkey PRIMARY KEY (id);


--
-- Name: actions actions_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.actions
    ADD CONSTRAINT actions_pkey PRIMARY KEY (id);


--
-- Name: addons addons_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.addons
    ADD CONSTRAINT addons_pkey PRIMARY KEY (id);


--
-- Name: api_token_project api_token_project_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.api_token_project
    ADD CONSTRAINT api_token_project_pkey PRIMARY KEY (secret, project);


--
-- Name: api_tokens api_tokens_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.api_tokens
    ADD CONSTRAINT api_tokens_pkey PRIMARY KEY (secret);


--
-- Name: change_request_approvals change_request_approvals_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_approvals
    ADD CONSTRAINT change_request_approvals_pkey PRIMARY KEY (id);


--
-- Name: change_request_comments change_request_comments_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_comments
    ADD CONSTRAINT change_request_comments_pkey PRIMARY KEY (id);


--
-- Name: change_request_events change_request_events_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_events
    ADD CONSTRAINT change_request_events_pkey PRIMARY KEY (id);


--
-- Name: change_request_rejections change_request_rejections_change_request_id_created_by_key; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_rejections
    ADD CONSTRAINT change_request_rejections_change_request_id_created_by_key UNIQUE (change_request_id, created_by);


--
-- Name: change_request_rejections change_request_rejections_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_rejections
    ADD CONSTRAINT change_request_rejections_pkey PRIMARY KEY (id);


--
-- Name: change_request_schedule change_request_schedule_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_schedule
    ADD CONSTRAINT change_request_schedule_pkey PRIMARY KEY (change_request);


--
-- Name: change_request_settings change_request_settings_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_settings
    ADD CONSTRAINT change_request_settings_pkey PRIMARY KEY (project, environment);


--
-- Name: change_request_settings change_request_settings_project_environment_key; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_settings
    ADD CONSTRAINT change_request_settings_project_environment_key UNIQUE (project, environment);


--
-- Name: change_requests change_requests_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_requests
    ADD CONSTRAINT change_requests_pkey PRIMARY KEY (id);


--
-- Name: client_applications client_applications_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.client_applications
    ADD CONSTRAINT client_applications_pkey PRIMARY KEY (app_name);


--
-- Name: client_applications_usage client_applications_usage_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.client_applications_usage
    ADD CONSTRAINT client_applications_usage_pkey PRIMARY KEY (app_name, project, environment);


--
-- Name: client_instances client_instances_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.client_instances
    ADD CONSTRAINT client_instances_pkey PRIMARY KEY (app_name, environment, instance_id);


--
-- Name: client_metrics_env_daily client_metrics_env_daily_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.client_metrics_env_daily
    ADD CONSTRAINT client_metrics_env_daily_pkey PRIMARY KEY (feature_name, app_name, environment, date);


--
-- Name: client_metrics_env client_metrics_env_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.client_metrics_env
    ADD CONSTRAINT client_metrics_env_pkey PRIMARY KEY (feature_name, app_name, environment, "timestamp");


--
-- Name: client_metrics_env_variants_daily client_metrics_env_variants_daily_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.client_metrics_env_variants_daily
    ADD CONSTRAINT client_metrics_env_variants_daily_pkey PRIMARY KEY (feature_name, app_name, environment, date, variant);


--
-- Name: client_metrics_env_variants client_metrics_env_variants_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.client_metrics_env_variants
    ADD CONSTRAINT client_metrics_env_variants_pkey PRIMARY KEY (feature_name, app_name, environment, "timestamp", variant);


--
-- Name: context_fields context_fields_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.context_fields
    ADD CONSTRAINT context_fields_pkey PRIMARY KEY (name);


--
-- Name: dependent_features dependent_features_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.dependent_features
    ADD CONSTRAINT dependent_features_pkey PRIMARY KEY (parent, child);


--
-- Name: environment_type_trends environment_type_trends_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.environment_type_trends
    ADD CONSTRAINT environment_type_trends_pkey PRIMARY KEY (id, environment_type);


--
-- Name: environments environments_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.environments
    ADD CONSTRAINT environments_pkey PRIMARY KEY (name);


--
-- Name: events events_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.events
    ADD CONSTRAINT events_pkey PRIMARY KEY (id);


--
-- Name: favorite_features favorite_features_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.favorite_features
    ADD CONSTRAINT favorite_features_pkey PRIMARY KEY (feature, user_id);


--
-- Name: favorite_projects favorite_projects_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.favorite_projects
    ADD CONSTRAINT favorite_projects_pkey PRIMARY KEY (project, user_id);


--
-- Name: feature_environments feature_environments_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.feature_environments
    ADD CONSTRAINT feature_environments_pkey PRIMARY KEY (environment, feature_name);


--
-- Name: feature_lifecycles feature_lifecycles_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.feature_lifecycles
    ADD CONSTRAINT feature_lifecycles_pkey PRIMARY KEY (feature, stage);


--
-- Name: feature_strategies feature_strategies_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.feature_strategies
    ADD CONSTRAINT feature_strategies_pkey PRIMARY KEY (id);


--
-- Name: feature_strategy_segment feature_strategy_segment_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.feature_strategy_segment
    ADD CONSTRAINT feature_strategy_segment_pkey PRIMARY KEY (feature_strategy_id, segment_id);


--
-- Name: feature_tag feature_tag_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.feature_tag
    ADD CONSTRAINT feature_tag_pkey PRIMARY KEY (feature_name, tag_type, tag_value);


--
-- Name: feature_types feature_types_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.feature_types
    ADD CONSTRAINT feature_types_pkey PRIMARY KEY (id);


--
-- Name: features features_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.features
    ADD CONSTRAINT features_pkey PRIMARY KEY (name);


--
-- Name: feedback feedback_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.feedback
    ADD CONSTRAINT feedback_pkey PRIMARY KEY (id);


--
-- Name: flag_trends flag_trends_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.flag_trends
    ADD CONSTRAINT flag_trends_pkey PRIMARY KEY (id, project);


--
-- Name: group_role group_role_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.group_role
    ADD CONSTRAINT group_role_pkey PRIMARY KEY (group_id, role_id, project);


--
-- Name: group_user group_user_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.group_user
    ADD CONSTRAINT group_user_pkey PRIMARY KEY (group_id, user_id);


--
-- Name: groups groups_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.groups
    ADD CONSTRAINT groups_pkey PRIMARY KEY (id);


--
-- Name: signal_endpoint_tokens incoming_webhook_tokens_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.signal_endpoint_tokens
    ADD CONSTRAINT incoming_webhook_tokens_pkey PRIMARY KEY (id);


--
-- Name: signal_endpoints incoming_webhooks_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.signal_endpoints
    ADD CONSTRAINT incoming_webhooks_pkey PRIMARY KEY (id);


--
-- Name: integration_events integration_events_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.integration_events
    ADD CONSTRAINT integration_events_pkey PRIMARY KEY (id);


--
-- Name: jobs jobs_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.jobs
    ADD CONSTRAINT jobs_pkey PRIMARY KEY (name, bucket);


--
-- Name: last_seen_at_metrics last_seen_at_metrics_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.last_seen_at_metrics
    ADD CONSTRAINT last_seen_at_metrics_pkey PRIMARY KEY (feature_name, environment);


--
-- Name: login_history login_events_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.login_history
    ADD CONSTRAINT login_events_pkey PRIMARY KEY (id);


--
-- Name: banners message_banners_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.banners
    ADD CONSTRAINT message_banners_pkey PRIMARY KEY (id);


--
-- Name: migrations migrations_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.migrations
    ADD CONSTRAINT migrations_pkey PRIMARY KEY (id);


--
-- Name: notifications notifications_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.notifications
    ADD CONSTRAINT notifications_pkey PRIMARY KEY (id);


--
-- Name: signals observable_events_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.signals
    ADD CONSTRAINT observable_events_pkey PRIMARY KEY (id);


--
-- Name: onboarding_events_instance onboarding_events_instance_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.onboarding_events_instance
    ADD CONSTRAINT onboarding_events_instance_pkey PRIMARY KEY (event);


--
-- Name: onboarding_events_project onboarding_events_project_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.onboarding_events_project
    ADD CONSTRAINT onboarding_events_project_pkey PRIMARY KEY (event, project);


--
-- Name: permissions permission_unique; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.permissions
    ADD CONSTRAINT permission_unique UNIQUE (permission);


--
-- Name: permissions permissions_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.permissions
    ADD CONSTRAINT permissions_pkey PRIMARY KEY (permission);


--
-- Name: personal_access_tokens personal_access_tokens_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.personal_access_tokens
    ADD CONSTRAINT personal_access_tokens_pkey PRIMARY KEY (id);


--
-- Name: project_client_metrics_trends project_client_metrics_trends_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.project_client_metrics_trends
    ADD CONSTRAINT project_client_metrics_trends_pkey PRIMARY KEY (project, date);


--
-- Name: project_environments project_environments_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.project_environments
    ADD CONSTRAINT project_environments_pkey PRIMARY KEY (project_id, environment_name);


--
-- Name: project_settings project_settings_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.project_settings
    ADD CONSTRAINT project_settings_pkey PRIMARY KEY (project);


--
-- Name: project_stats project_stats_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.project_stats
    ADD CONSTRAINT project_stats_pkey PRIMARY KEY (project);


--
-- Name: project_stats project_stats_project_key; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.project_stats
    ADD CONSTRAINT project_stats_project_key UNIQUE (project);


--
-- Name: projects projects_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.projects
    ADD CONSTRAINT projects_pkey PRIMARY KEY (id);


--
-- Name: public_signup_tokens public_signup_tokens_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.public_signup_tokens
    ADD CONSTRAINT public_signup_tokens_pkey PRIMARY KEY (secret);


--
-- Name: public_signup_tokens_user public_signup_tokens_user_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.public_signup_tokens_user
    ADD CONSTRAINT public_signup_tokens_user_pkey PRIMARY KEY (secret, user_id);


--
-- Name: reset_tokens reset_tokens_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.reset_tokens
    ADD CONSTRAINT reset_tokens_pkey PRIMARY KEY (reset_token);


--
-- Name: role_permission role_permission_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.role_permission
    ADD CONSTRAINT role_permission_pkey PRIMARY KEY (id);


--
-- Name: role_user role_user_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.role_user
    ADD CONSTRAINT role_user_pkey PRIMARY KEY (role_id, user_id, project);


--
-- Name: roles roles_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT roles_pkey PRIMARY KEY (id);


--
-- Name: segments segments_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.segments
    ADD CONSTRAINT segments_pkey PRIMARY KEY (id);


--
-- Name: settings settings_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.settings
    ADD CONSTRAINT settings_pkey PRIMARY KEY (name);


--
-- Name: stat_environment_updates stat_environment_updates_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.stat_environment_updates
    ADD CONSTRAINT stat_environment_updates_pkey PRIMARY KEY (day, environment);


--
-- Name: stat_traffic_usage stat_traffic_usage_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.stat_traffic_usage
    ADD CONSTRAINT stat_traffic_usage_pkey PRIMARY KEY (day, traffic_group, status_code_series);


--
-- Name: strategies strategies_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.strategies
    ADD CONSTRAINT strategies_pkey PRIMARY KEY (name);


--
-- Name: tag_types tag_types_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.tag_types
    ADD CONSTRAINT tag_types_pkey PRIMARY KEY (name);


--
-- Name: tags tags_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.tags
    ADD CONSTRAINT tags_pkey PRIMARY KEY (type, value);


--
-- Name: change_request_approvals unique_approvals; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_approvals
    ADD CONSTRAINT unique_approvals UNIQUE (change_request_id, created_by);


--
-- Name: roles unique_name; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT unique_name UNIQUE (name);


--
-- Name: unleash_session unleash_session_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.unleash_session
    ADD CONSTRAINT unleash_session_pkey PRIMARY KEY (sid);


--
-- Name: used_passwords used_passwords_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.used_passwords
    ADD CONSTRAINT used_passwords_pkey PRIMARY KEY (user_id, password_hash);


--
-- Name: user_feedback user_feedback_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.user_feedback
    ADD CONSTRAINT user_feedback_pkey PRIMARY KEY (user_id, feedback_id);


--
-- Name: user_notifications user_notifications_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.user_notifications
    ADD CONSTRAINT user_notifications_pkey PRIMARY KEY (notification_id, user_id);


--
-- Name: user_splash user_splash_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.user_splash
    ADD CONSTRAINT user_splash_pkey PRIMARY KEY (user_id, splash_id);


--
-- Name: user_trends user_trends_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.user_trends
    ADD CONSTRAINT user_trends_pkey PRIMARY KEY (id);


--
-- Name: users users_email_key; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: users users_username_key; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_username_key UNIQUE (username);


--
-- Name: client_instances_environment_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX client_instances_environment_idx ON public.client_instances USING btree (environment);


--
-- Name: events_created_by_user_id_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX events_created_by_user_id_idx ON public.events USING btree (created_by_user_id);


--
-- Name: events_environment_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX events_environment_idx ON public.events USING btree (environment);


--
-- Name: events_project_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX events_project_idx ON public.events USING btree (project);


--
-- Name: events_unannounced_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX events_unannounced_idx ON public.events USING btree (announced) WHERE (announced = false);


--
-- Name: feature_environments_feature_name_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX feature_environments_feature_name_idx ON public.feature_environments USING btree (feature_name);


--
-- Name: feature_name_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX feature_name_idx ON public.events USING btree (feature_name);


--
-- Name: feature_strategies_environment_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX feature_strategies_environment_idx ON public.feature_strategies USING btree (environment);


--
-- Name: feature_strategies_feature_name_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX feature_strategies_feature_name_idx ON public.feature_strategies USING btree (feature_name);


--
-- Name: feature_strategy_segment_segment_id_index; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX feature_strategy_segment_segment_id_index ON public.feature_strategy_segment USING btree (segment_id);


--
-- Name: feature_tag_tag_type_and_value_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX feature_tag_tag_type_and_value_idx ON public.feature_tag USING btree (tag_type, tag_value);


--
-- Name: groups_group_name_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX groups_group_name_idx ON public.groups USING btree (name);


--
-- Name: groups_scim_external_id_uniq_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE UNIQUE INDEX groups_scim_external_id_uniq_idx ON public.groups USING btree (scim_external_id) WHERE (scim_external_id IS NOT NULL);


--
-- Name: groups_scim_id_unique_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE UNIQUE INDEX groups_scim_id_unique_idx ON public.groups USING btree (scim_id) WHERE (scim_id IS NOT NULL);


--
-- Name: idx_action_set_events_action_set_id_state; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_action_set_events_action_set_id_state ON public.action_set_events USING btree (action_set_id, state);


--
-- Name: idx_action_set_events_signal_id; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_action_set_events_signal_id ON public.action_set_events USING btree (signal_id);


--
-- Name: idx_action_sets_project; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_action_sets_project ON public.action_sets USING btree (project);


--
-- Name: idx_client_applications_announced_false; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_client_applications_announced_false ON public.client_applications USING btree (announced) WHERE (announced = false);


--
-- Name: idx_client_metrics_f_name; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_client_metrics_f_name ON public.client_metrics_env USING btree (feature_name);


--
-- Name: idx_events_created_at_desc; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_events_created_at_desc ON public.events USING btree (created_at DESC);


--
-- Name: idx_events_feature_type_id; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_events_feature_type_id ON public.events USING btree (id) WHERE ((feature_name IS NOT NULL) OR ((type)::text = ANY ((ARRAY['segment-updated'::character varying, 'feature_import'::character varying, 'features-imported'::character varying])::text[])));


--
-- Name: idx_events_type; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_events_type ON public.events USING btree (type);


--
-- Name: idx_feature_name; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_feature_name ON public.last_seen_at_metrics USING btree (feature_name);


--
-- Name: idx_integration_events_integration_id; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_integration_events_integration_id ON public.integration_events USING btree (integration_id);


--
-- Name: idx_job_finished; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_job_finished ON public.jobs USING btree (finished_at);


--
-- Name: idx_job_stage; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_job_stage ON public.jobs USING btree (stage);


--
-- Name: idx_signal_endpoint_tokens_signal_endpoint_id; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_signal_endpoint_tokens_signal_endpoint_id ON public.signal_endpoint_tokens USING btree (signal_endpoint_id);


--
-- Name: idx_signal_endpoints_enabled; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_signal_endpoints_enabled ON public.signal_endpoints USING btree (enabled);


--
-- Name: idx_signals_created_by_source_token_id; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_signals_created_by_source_token_id ON public.signals USING btree (created_by_source_token_id);


--
-- Name: idx_signals_source_and_source_id; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_signals_source_and_source_id ON public.signals USING btree (source, source_id);


--
-- Name: idx_signals_unannounced; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_signals_unannounced ON public.signals USING btree (announced) WHERE (announced = false);


--
-- Name: idx_unleash_session_expired; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX idx_unleash_session_expired ON public.unleash_session USING btree (expired);


--
-- Name: login_events_ip_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX login_events_ip_idx ON public.login_history USING btree (ip);


--
-- Name: project_environments_environment_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX project_environments_environment_idx ON public.project_environments USING btree (environment_name);


--
-- Name: reset_tokens_user_id_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX reset_tokens_user_id_idx ON public.reset_tokens USING btree (user_id);


--
-- Name: role_permission_role_id_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX role_permission_role_id_idx ON public.role_permission USING btree (role_id);


--
-- Name: role_user_user_id_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX role_user_user_id_idx ON public.role_user USING btree (user_id);


--
-- Name: segments_name_index; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX segments_name_index ON public.segments USING btree (name);


--
-- Name: stat_traffic_usage_day_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX stat_traffic_usage_day_idx ON public.stat_traffic_usage USING btree (day);


--
-- Name: stat_traffic_usage_status_code_series_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX stat_traffic_usage_status_code_series_idx ON public.stat_traffic_usage USING btree (status_code_series);


--
-- Name: stat_traffic_usage_traffic_group_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX stat_traffic_usage_traffic_group_idx ON public.stat_traffic_usage USING btree (traffic_group);


--
-- Name: used_passwords_pw_hash_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX used_passwords_pw_hash_idx ON public.used_passwords USING btree (password_hash);


--
-- Name: user_feedback_user_id_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX user_feedback_user_id_idx ON public.user_feedback USING btree (user_id);


--
-- Name: user_splash_user_id_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE INDEX user_splash_user_id_idx ON public.user_splash USING btree (user_id);


--
-- Name: users_scim_external_id_uniq_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE UNIQUE INDEX users_scim_external_id_uniq_idx ON public.users USING btree (scim_external_id) WHERE (scim_external_id IS NOT NULL);


--
-- Name: users_scim_id_unique_idx; Type: INDEX; Schema: public; Owner: root
--

CREATE UNIQUE INDEX users_scim_id_unique_idx ON public.users USING btree (scim_id) WHERE (scim_id IS NOT NULL);


--
-- Name: events unleash_update_stat_environment_changes; Type: TRIGGER; Schema: public; Owner: root
--

CREATE TRIGGER unleash_update_stat_environment_changes AFTER INSERT ON public.events FOR EACH ROW EXECUTE FUNCTION public.unleash_update_stat_environment_changes_counter();


--
-- Name: action_sets action_sets_project_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.action_sets
    ADD CONSTRAINT action_sets_project_fkey FOREIGN KEY (project) REFERENCES public.projects(id) ON DELETE CASCADE;


--
-- Name: actions actions_action_set_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.actions
    ADD CONSTRAINT actions_action_set_id_fkey FOREIGN KEY (action_set_id) REFERENCES public.action_sets(id) ON DELETE CASCADE;


--
-- Name: api_token_project api_token_project_project_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.api_token_project
    ADD CONSTRAINT api_token_project_project_fkey FOREIGN KEY (project) REFERENCES public.projects(id) ON DELETE CASCADE;


--
-- Name: api_token_project api_token_project_secret_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.api_token_project
    ADD CONSTRAINT api_token_project_secret_fkey FOREIGN KEY (secret) REFERENCES public.api_tokens(secret) ON DELETE CASCADE;


--
-- Name: change_request_approvals change_request_approvals_change_request_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_approvals
    ADD CONSTRAINT change_request_approvals_change_request_id_fkey FOREIGN KEY (change_request_id) REFERENCES public.change_requests(id) ON DELETE CASCADE;


--
-- Name: change_request_approvals change_request_approvals_created_by_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_approvals
    ADD CONSTRAINT change_request_approvals_created_by_fkey FOREIGN KEY (created_by) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: change_request_comments change_request_comments_change_request_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_comments
    ADD CONSTRAINT change_request_comments_change_request_fkey FOREIGN KEY (change_request) REFERENCES public.change_requests(id) ON DELETE CASCADE;


--
-- Name: change_request_comments change_request_comments_created_by_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_comments
    ADD CONSTRAINT change_request_comments_created_by_fkey FOREIGN KEY (created_by) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: change_request_events change_request_events_change_request_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_events
    ADD CONSTRAINT change_request_events_change_request_id_fkey FOREIGN KEY (change_request_id) REFERENCES public.change_requests(id) ON DELETE CASCADE;


--
-- Name: change_request_events change_request_events_created_by_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_events
    ADD CONSTRAINT change_request_events_created_by_fkey FOREIGN KEY (created_by) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: change_request_events change_request_events_feature_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_events
    ADD CONSTRAINT change_request_events_feature_fkey FOREIGN KEY (feature) REFERENCES public.features(name) ON DELETE CASCADE;


--
-- Name: change_request_rejections change_request_rejections_change_request_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_rejections
    ADD CONSTRAINT change_request_rejections_change_request_id_fkey FOREIGN KEY (change_request_id) REFERENCES public.change_requests(id) ON DELETE CASCADE;


--
-- Name: change_request_rejections change_request_rejections_created_by_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_rejections
    ADD CONSTRAINT change_request_rejections_created_by_fkey FOREIGN KEY (created_by) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: change_request_schedule change_request_schedule_change_request_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_schedule
    ADD CONSTRAINT change_request_schedule_change_request_fkey FOREIGN KEY (change_request) REFERENCES public.change_requests(id) ON DELETE CASCADE;


--
-- Name: change_request_schedule change_request_schedule_created_by_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_schedule
    ADD CONSTRAINT change_request_schedule_created_by_fkey FOREIGN KEY (created_by) REFERENCES public.users(id);


--
-- Name: change_request_settings change_request_settings_environment_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_settings
    ADD CONSTRAINT change_request_settings_environment_fkey FOREIGN KEY (environment) REFERENCES public.environments(name) ON DELETE CASCADE;


--
-- Name: change_request_settings change_request_settings_project_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_request_settings
    ADD CONSTRAINT change_request_settings_project_fkey FOREIGN KEY (project) REFERENCES public.projects(id) ON DELETE CASCADE;


--
-- Name: change_requests change_requests_created_by_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_requests
    ADD CONSTRAINT change_requests_created_by_fkey FOREIGN KEY (created_by) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: change_requests change_requests_environment_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_requests
    ADD CONSTRAINT change_requests_environment_fkey FOREIGN KEY (environment) REFERENCES public.environments(name) ON DELETE CASCADE;


--
-- Name: change_requests change_requests_project_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.change_requests
    ADD CONSTRAINT change_requests_project_fkey FOREIGN KEY (project) REFERENCES public.projects(id) ON DELETE CASCADE;


--
-- Name: client_applications_usage client_applications_usage_app_name_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.client_applications_usage
    ADD CONSTRAINT client_applications_usage_app_name_fkey FOREIGN KEY (app_name) REFERENCES public.client_applications(app_name) ON DELETE CASCADE;


--
-- Name: client_metrics_env_variants_daily client_metrics_env_variants_d_feature_name_app_name_enviro_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.client_metrics_env_variants_daily
    ADD CONSTRAINT client_metrics_env_variants_d_feature_name_app_name_enviro_fkey FOREIGN KEY (feature_name, app_name, environment, date) REFERENCES public.client_metrics_env_daily(feature_name, app_name, environment, date) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: client_metrics_env_variants client_metrics_env_variants_feature_name_app_name_environm_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.client_metrics_env_variants
    ADD CONSTRAINT client_metrics_env_variants_feature_name_app_name_environm_fkey FOREIGN KEY (feature_name, app_name, environment, "timestamp") REFERENCES public.client_metrics_env(feature_name, app_name, environment, "timestamp") ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: dependent_features dependent_features_child_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.dependent_features
    ADD CONSTRAINT dependent_features_child_fkey FOREIGN KEY (child) REFERENCES public.features(name) ON DELETE CASCADE;


--
-- Name: dependent_features dependent_features_parent_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.dependent_features
    ADD CONSTRAINT dependent_features_parent_fkey FOREIGN KEY (parent) REFERENCES public.features(name) ON DELETE RESTRICT;


--
-- Name: favorite_features favorite_features_feature_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.favorite_features
    ADD CONSTRAINT favorite_features_feature_fkey FOREIGN KEY (feature) REFERENCES public.features(name) ON DELETE CASCADE;


--
-- Name: favorite_features favorite_features_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.favorite_features
    ADD CONSTRAINT favorite_features_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: favorite_projects favorite_projects_project_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.favorite_projects
    ADD CONSTRAINT favorite_projects_project_fkey FOREIGN KEY (project) REFERENCES public.projects(id) ON DELETE CASCADE;


--
-- Name: favorite_projects favorite_projects_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.favorite_projects
    ADD CONSTRAINT favorite_projects_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: feature_environments feature_environments_environment_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.feature_environments
    ADD CONSTRAINT feature_environments_environment_fkey FOREIGN KEY (environment) REFERENCES public.environments(name) ON DELETE CASCADE;


--
-- Name: feature_environments feature_environments_feature_name_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.feature_environments
    ADD CONSTRAINT feature_environments_feature_name_fkey FOREIGN KEY (feature_name) REFERENCES public.features(name) ON DELETE CASCADE;


--
-- Name: feature_lifecycles feature_lifecycles_feature_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.feature_lifecycles
    ADD CONSTRAINT feature_lifecycles_feature_fkey FOREIGN KEY (feature) REFERENCES public.features(name) ON DELETE CASCADE;


--
-- Name: feature_strategies feature_strategies_environment_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.feature_strategies
    ADD CONSTRAINT feature_strategies_environment_fkey FOREIGN KEY (environment) REFERENCES public.environments(name) ON DELETE CASCADE;


--
-- Name: feature_strategies feature_strategies_feature_name_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.feature_strategies
    ADD CONSTRAINT feature_strategies_feature_name_fkey FOREIGN KEY (feature_name) REFERENCES public.features(name) ON DELETE CASCADE;


--
-- Name: feature_strategy_segment feature_strategy_segment_feature_strategy_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.feature_strategy_segment
    ADD CONSTRAINT feature_strategy_segment_feature_strategy_id_fkey FOREIGN KEY (feature_strategy_id) REFERENCES public.feature_strategies(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: feature_strategy_segment feature_strategy_segment_segment_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.feature_strategy_segment
    ADD CONSTRAINT feature_strategy_segment_segment_id_fkey FOREIGN KEY (segment_id) REFERENCES public.segments(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: feature_tag feature_tag_feature_name_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.feature_tag
    ADD CONSTRAINT feature_tag_feature_name_fkey FOREIGN KEY (feature_name) REFERENCES public.features(name) ON DELETE CASCADE;


--
-- Name: feature_tag feature_tag_tag_type_tag_value_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.feature_tag
    ADD CONSTRAINT feature_tag_tag_type_tag_value_fkey FOREIGN KEY (tag_type, tag_value) REFERENCES public.tags(type, value) ON DELETE CASCADE;


--
-- Name: groups fk_group_role_id; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.groups
    ADD CONSTRAINT fk_group_role_id FOREIGN KEY (root_role_id) REFERENCES public.roles(id);


--
-- Name: group_role fk_group_role_project; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.group_role
    ADD CONSTRAINT fk_group_role_project FOREIGN KEY (project) REFERENCES public.projects(id) ON DELETE CASCADE;


--
-- Name: role_permission fk_role_permission_permission; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.role_permission
    ADD CONSTRAINT fk_role_permission_permission FOREIGN KEY (permission) REFERENCES public.permissions(permission) ON DELETE CASCADE;


--
-- Name: group_role group_role_group_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.group_role
    ADD CONSTRAINT group_role_group_id_fkey FOREIGN KEY (group_id) REFERENCES public.groups(id) ON DELETE CASCADE;


--
-- Name: group_role group_role_role_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.group_role
    ADD CONSTRAINT group_role_role_id_fkey FOREIGN KEY (role_id) REFERENCES public.roles(id) ON DELETE CASCADE;


--
-- Name: group_user group_user_group_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.group_user
    ADD CONSTRAINT group_user_group_id_fkey FOREIGN KEY (group_id) REFERENCES public.groups(id) ON DELETE CASCADE;


--
-- Name: group_user group_user_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.group_user
    ADD CONSTRAINT group_user_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: signal_endpoint_tokens incoming_webhook_tokens_incoming_webhook_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.signal_endpoint_tokens
    ADD CONSTRAINT incoming_webhook_tokens_incoming_webhook_id_fkey FOREIGN KEY (signal_endpoint_id) REFERENCES public.signal_endpoints(id) ON DELETE CASCADE;


--
-- Name: integration_events integration_events_integration_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.integration_events
    ADD CONSTRAINT integration_events_integration_id_fkey FOREIGN KEY (integration_id) REFERENCES public.addons(id) ON DELETE CASCADE;


--
-- Name: notifications notifications_event_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.notifications
    ADD CONSTRAINT notifications_event_id_fkey FOREIGN KEY (event_id) REFERENCES public.events(id) ON DELETE CASCADE;


--
-- Name: onboarding_events_project onboarding_events_project_project_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.onboarding_events_project
    ADD CONSTRAINT onboarding_events_project_project_fkey FOREIGN KEY (project) REFERENCES public.projects(id) ON DELETE CASCADE;


--
-- Name: personal_access_tokens personal_access_tokens_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.personal_access_tokens
    ADD CONSTRAINT personal_access_tokens_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: project_environments project_environments_environment_name_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.project_environments
    ADD CONSTRAINT project_environments_environment_name_fkey FOREIGN KEY (environment_name) REFERENCES public.environments(name) ON DELETE CASCADE;


--
-- Name: project_environments project_environments_project_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.project_environments
    ADD CONSTRAINT project_environments_project_id_fkey FOREIGN KEY (project_id) REFERENCES public.projects(id) ON DELETE CASCADE;


--
-- Name: project_settings project_settings_project_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.project_settings
    ADD CONSTRAINT project_settings_project_fkey FOREIGN KEY (project) REFERENCES public.projects(id) ON DELETE CASCADE;


--
-- Name: project_stats project_stats_project_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.project_stats
    ADD CONSTRAINT project_stats_project_fkey FOREIGN KEY (project) REFERENCES public.projects(id) ON DELETE CASCADE;


--
-- Name: public_signup_tokens public_signup_tokens_role_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.public_signup_tokens
    ADD CONSTRAINT public_signup_tokens_role_id_fkey FOREIGN KEY (role_id) REFERENCES public.roles(id) ON DELETE CASCADE;


--
-- Name: public_signup_tokens_user public_signup_tokens_user_secret_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.public_signup_tokens_user
    ADD CONSTRAINT public_signup_tokens_user_secret_fkey FOREIGN KEY (secret) REFERENCES public.public_signup_tokens(secret) ON DELETE CASCADE;


--
-- Name: public_signup_tokens_user public_signup_tokens_user_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.public_signup_tokens_user
    ADD CONSTRAINT public_signup_tokens_user_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: reset_tokens reset_tokens_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.reset_tokens
    ADD CONSTRAINT reset_tokens_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: role_permission role_permission_role_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.role_permission
    ADD CONSTRAINT role_permission_role_id_fkey FOREIGN KEY (role_id) REFERENCES public.roles(id) ON DELETE CASCADE;


--
-- Name: role_user role_user_role_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.role_user
    ADD CONSTRAINT role_user_role_id_fkey FOREIGN KEY (role_id) REFERENCES public.roles(id) ON DELETE CASCADE;


--
-- Name: role_user role_user_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.role_user
    ADD CONSTRAINT role_user_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: segments segments_segment_project_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.segments
    ADD CONSTRAINT segments_segment_project_id_fkey FOREIGN KEY (segment_project_id) REFERENCES public.projects(id) ON DELETE CASCADE;


--
-- Name: tags tags_type_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.tags
    ADD CONSTRAINT tags_type_fkey FOREIGN KEY (type) REFERENCES public.tag_types(name) ON DELETE CASCADE;


--
-- Name: used_passwords used_passwords_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.used_passwords
    ADD CONSTRAINT used_passwords_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: user_feedback user_feedback_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.user_feedback
    ADD CONSTRAINT user_feedback_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: user_notifications user_notifications_notification_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.user_notifications
    ADD CONSTRAINT user_notifications_notification_id_fkey FOREIGN KEY (notification_id) REFERENCES public.notifications(id) ON DELETE CASCADE;


--
-- Name: user_notifications user_notifications_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.user_notifications
    ADD CONSTRAINT user_notifications_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: user_splash user_splash_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.user_splash
    ADD CONSTRAINT user_splash_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

